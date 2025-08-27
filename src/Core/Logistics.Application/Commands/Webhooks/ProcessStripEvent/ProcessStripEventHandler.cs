using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using PaymentMethod = Logistics.Domain.Entities.PaymentMethod;
using StripeInvoice = Stripe.Invoice;
using StripePaymentMethod = Stripe.PaymentMethod;
using StripeSubscription = Stripe.Subscription;
using Subscription = Logistics.Domain.Entities.Subscription;

namespace Logistics.Application.Commands;

internal sealed class ProcessStripEventHandler : IAppRequestHandler<ProcessStripEventCommand, Result>
{
    private readonly ILogger<ProcessStripEventHandler> _logger;
    private readonly IMasterUnitOfWork _masterUow;
    private readonly IStripeService _stripeService;
    private readonly string _stripeWebhookSecret;
    private readonly ITenantUnitOfWork _tenantUow;

    public ProcessStripEventHandler(
        ITenantUnitOfWork tenantUow,
        IMasterUnitOfWork masterUow,
        IStripeService stripeService,
        IOptions<StripeOptions> stripeOptions,
        ILogger<ProcessStripEventHandler> logger)
    {
        _tenantUow = tenantUow;
        _masterUow = masterUow;
        _stripeService = stripeService;
        _logger = logger;
        _stripeWebhookSecret =
            stripeOptions.Value.WebhookSecret ?? throw new ArgumentNullException(nameof(stripeOptions));
    }

    public async Task<Result> Handle(
        ProcessStripEventCommand req, CancellationToken ct)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                req.RequestBodyJson,
                req.StripeSignature,
                _stripeWebhookSecret,
                throwOnApiVersionMismatch: false);

            _logger.LogInformation("Received Stripe event: {Type}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case EventTypes.InvoicePaid:
                    return await HandleInvoicePaid((stripeEvent.Data.Object as StripeInvoice)!);
                case EventTypes.PaymentMethodAttached:
                    return await HandlePaymentMethodAttached((stripeEvent.Data.Object as StripePaymentMethod)!);
                case EventTypes.PaymentMethodDetached:
                    return await HandlePaymentMethodDetached((stripeEvent.Data.Object as StripePaymentMethod)!);
                case EventTypes.CustomerSubscriptionCreated:
                    return await HandleSubscriptionCreated((stripeEvent.Data.Object as StripeSubscription)!);
                case EventTypes.CustomerSubscriptionUpdated:
                    return await HandleSubscriptionUpdated((stripeEvent.Data.Object as StripeSubscription)!);
                case EventTypes.CustomerSubscriptionDeleted:
                    return await HandleSubscriptionDeleted((stripeEvent.Data.Object as StripeSubscription)!);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe error: {Message}", ex.Message);
            return Result.Fail(ex.Message);
        }
    }


    #region Invoice Handlers

    private async Task<Result> HandleInvoicePaid(StripeInvoice stripeInvoice)
    {
        var customer = await _stripeService.GetCustomerAsync(stripeInvoice.CustomerId);
        if (!customer.Metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in invoice metadata.");
        }

        if (!Guid.TryParse(tenantId, out var parsedTenantId))
        {
            return Result.Fail($"Invalid tenant ID format: {tenantId}");
        }

        await _tenantUow.SetCurrentTenantByIdAsync(parsedTenantId);

        var amount = stripeInvoice.AmountPaid / 100m; // Convert from cents
        var stripePaymentMethodId = stripeInvoice.DefaultPaymentMethodId;

        var paymentMethod = await _tenantUow.Repository<PaymentMethod>()
            .GetAsync(pm => pm.StripePaymentMethodId == stripePaymentMethodId);

        if (paymentMethod is null)
        {
            return Result.Fail($"Payment method {stripePaymentMethodId} not found for tenant {tenantId}.");
        }

        var tenant = _tenantUow.GetCurrentTenant();

        var payment = new Payment
        {
            Amount = new Money { Amount = amount, Currency = stripeInvoice.Currency },
            MethodId = paymentMethod.Id,
            TenantId = tenant.Id,
            Status = PaymentStatus.Paid,
            BillingAddress = stripeInvoice.CustomerAddress.ToAddressEntity()
        };

        await _tenantUow.Repository<Payment>().AddAsync(payment);
        await _tenantUow.SaveChangesAsync();

        _logger.LogInformation("Added payment for tenant {TenantId} with amount {Amount}, invoice ID {StripeInvoiceId}",
            tenantId, payment.Amount, stripeInvoice.Id);
        return Result.Ok();
    }

    #endregion


    #region Subscription Handlers

    private async Task<Result> HandleSubscriptionCreated(StripeSubscription stripeSubscription)
    {
        var metadata = stripeSubscription.Metadata;
        if (!metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in subscription metadata.");
        }

        if (!metadata.TryGetValue(StripeMetadataKeys.PlanId, out var planId))
        {
            return Result.Fail($"{StripeMetadataKeys.PlanId} not found in subscription metadata.");
        }

        var tenant = await _masterUow.Repository<Tenant>().GetByIdAsync(Guid.Parse(tenantId));

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{tenantId}'");
        }

        var subscriptionPlan = await _masterUow.Repository<SubscriptionPlan>().GetByIdAsync(Guid.Parse(planId));

        if (subscriptionPlan is null)
        {
            return Result.Fail($"Could not find a subscription plan with ID '{planId}'");
        }

        if (tenant.Subscription is not null)
        {
            tenant.Subscription.Status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
            tenant.Subscription.StripeSubscriptionId = stripeSubscription.Id;
            tenant.Subscription.StartDate = stripeSubscription.StartDate;
            tenant.Subscription.NextBillingDate = stripeSubscription.Items.Data.First().CurrentPeriodEnd;
            tenant.Subscription.TrialEndDate = stripeSubscription.TrialEnd;

            _masterUow.Repository<Subscription>().Update(tenant.Subscription);
            _logger.LogInformation("Updated existing subscription {StripeSubscriptionId} for tenant {TenantId}",
                stripeSubscription.Id, tenantId);
        }
        else
        {
            var newSubscription = Subscription.CreateTrial(tenant, subscriptionPlan);
            newSubscription.StripeSubscriptionId = stripeSubscription.Id;
            newSubscription.StartDate = stripeSubscription.StartDate;
            newSubscription.NextBillingDate = stripeSubscription.Items.Data.First().CurrentPeriodEnd;
            newSubscription.TrialEndDate = stripeSubscription.TrialEnd;

            await _masterUow.Repository<Subscription>().AddAsync(newSubscription);
            _logger.LogInformation("Created new subscription {StripeSubscriptionId} for tenant {TenantId}",
                stripeSubscription.Id, tenantId);
        }

        await _masterUow.SaveChangesAsync();
        return Result.Ok();
    }

    private async Task<Result> HandleSubscriptionUpdated(StripeSubscription stripeSubscription)
    {
        var metadata = stripeSubscription.Metadata;
        if (!metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in subscription metadata.");
        }

        var subscription = await _masterUow.Repository<Subscription>()
            .GetAsync(s => s.StripeSubscriptionId == stripeSubscription.Id);

        if (subscription is null)
        {
            return Result.Fail($"Subscription {stripeSubscription.Id} not found for tenant {tenantId}.");
        }

        subscription.Status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
        subscription.StartDate = stripeSubscription.StartDate;
        subscription.NextBillingDate = stripeSubscription.Items.Data.First().CurrentPeriodEnd;
        subscription.TrialEndDate = stripeSubscription.TrialEnd;

        _masterUow.Repository<Subscription>().Update(subscription);
        await _masterUow.SaveChangesAsync();

        _logger.LogInformation("Updated subscription {StripeSubscriptionId} for tenant {TenantId}",
            stripeSubscription.Id, tenantId);
        return Result.Ok();
    }

    private async Task<Result> HandleSubscriptionDeleted(StripeSubscription stripeSubscription)
    {
        if (!stripeSubscription.Metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in subscription metadata.");
        }

        var subscription = await _masterUow.Repository<Subscription>()
            .GetAsync(s => s.StripeSubscriptionId == stripeSubscription.Id);

        if (subscription is null)
        {
            return Result.Fail($"Subscription {stripeSubscription.Id} not found for tenant {tenantId}.");
        }

        subscription.Status = StripeObjectMapper.GetSubscriptionStatus(stripeSubscription.Status);
        subscription.EndDate = stripeSubscription.EndedAt;
        _masterUow.Repository<Subscription>().Update(subscription);
        await _masterUow.SaveChangesAsync();

        _logger.LogInformation("Cancelled subscription {StripeSubscriptionId} for tenant {TenantId}",
            stripeSubscription.Id, tenantId);
        return Result.Ok();
    }

    #endregion


    #region Payment Method Handlers

    private async Task<Result> HandlePaymentMethodAttached(StripePaymentMethod stripePaymentMethod)
    {
        if (!stripePaymentMethod.Metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in payment method metadata.");
        }

        if (!Guid.TryParse(tenantId, out var parsedTenantId))
        {
            return Result.Fail($"Invalid tenant ID format: {tenantId}");
        }

        await _tenantUow.SetCurrentTenantByIdAsync(parsedTenantId);
        var paymentMethod = stripePaymentMethod.ToPaymentMethodEntity();

        if (paymentMethod is UsBankAccountPaymentMethod usBankAccountPaymentMethod)
        {
            await UpdateOrAddUsBankAccount(usBankAccountPaymentMethod);
        }
        else
        {
            await _tenantUow.Repository<PaymentMethod>().AddAsync(paymentMethod);
        }

        await _tenantUow.SaveChangesAsync();
        _logger.LogInformation(
            "Attached payment method {PaymentMethodType} {StripePaymentMethodId} to tenant {TenantId}",
            paymentMethod.Type, paymentMethod.StripePaymentMethodId, tenantId);
        return Result.Ok();
    }

    private async Task<Result> HandlePaymentMethodDetached(StripePaymentMethod stripePaymentMethod)
    {
        if (!stripePaymentMethod.Metadata.TryGetValue(StripeMetadataKeys.TenantId, out var tenantId))
        {
            return Result.Fail($"{StripeMetadataKeys.TenantId} not found in payment method metadata.");
        }

        if (!Guid.TryParse(tenantId, out var parsedTenantId))
        {
            return Result.Fail($"Invalid tenant ID format: {tenantId}");
        }

        await _tenantUow.SetCurrentTenantByIdAsync(parsedTenantId);
        var paymentMethod = await _tenantUow.Repository<PaymentMethod>()
            .GetAsync(pm => pm.StripePaymentMethodId == stripePaymentMethod.Id);

        if (paymentMethod is null)
        {
            return Result.Fail($"Payment method {stripePaymentMethod.Id} not found for tenant {tenantId}.");
        }

        _tenantUow.Repository<PaymentMethod>().Delete(paymentMethod);
        await _tenantUow.SaveChangesAsync();

        _logger.LogInformation(
            "Detached payment method {PaymentMethodType} {StripePaymentMethodId} from tenant {TenantId}",
            paymentMethod.Type, paymentMethod.StripePaymentMethodId, tenantId);
        return Result.Ok();
    }

    /// <summary>
    ///     Checks if the US bank account payment method is already in the database and updates its verification status.
    ///     Otherwise, it adds the payment method to the database.
    /// </summary>
    /// <param name="paymentMethod">The US bank account payment method to handle.</param>
    private async Task UpdateOrAddUsBankAccount(UsBankAccountPaymentMethod paymentMethod)
    {
        var existingPaymentMethod = await _tenantUow.Repository<PaymentMethod>()
            .GetAsync(pm => pm.StripePaymentMethodId == paymentMethod.StripePaymentMethodId);

        if (existingPaymentMethod is not UsBankAccountPaymentMethod usBankAccountPaymentMethod)
        {
            await _tenantUow.Repository<PaymentMethod>().AddAsync(paymentMethod);
            return;
        }

        // Update the existing payment method with verified details
        usBankAccountPaymentMethod.BankName = paymentMethod.BankName;
        usBankAccountPaymentMethod.AccountNumber = usBankAccountPaymentMethod.AccountNumber;
        usBankAccountPaymentMethod.RoutingNumber = paymentMethod.RoutingNumber;
        usBankAccountPaymentMethod.AccountHolderType = paymentMethod.AccountHolderType;
        usBankAccountPaymentMethod.AccountType = paymentMethod.AccountType;
        usBankAccountPaymentMethod.VerificationStatus = PaymentMethodVerificationStatus.Verified;

        _tenantUow.Repository<PaymentMethod>().Update(usBankAccountPaymentMethod);
        _logger.LogInformation("Verified US bank account payment method {StripePaymentMethodId} for tenant {TenantId}",
            paymentMethod.StripePaymentMethodId, _tenantUow.GetCurrentTenant().Id);
    }

    #endregion
}
