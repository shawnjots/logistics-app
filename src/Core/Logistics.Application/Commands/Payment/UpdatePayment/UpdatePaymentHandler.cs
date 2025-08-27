using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdatePaymentHandler : IAppRequestHandler<UpdatePaymentCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public UpdatePaymentHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result> Handle(
        UpdatePaymentCommand req, CancellationToken ct)
    {
        var payment = await _tenantUow.Repository<Payment>().GetByIdAsync(req.Id);

        if (payment is null)
        {
            return Result.Fail($"Could not find a payment with ID '{req.Id}'");
        }

        var updateResult = await TryUpdatePaymentMethod(payment, req.PaymentMethodId);

        if (!updateResult.Success)
        {
            return updateResult;
        }

        payment.Status = PropertyUpdater.UpdateIfChanged(req.Status, payment.Status);
        payment.Amount = PropertyUpdater.UpdateIfChanged(req.Amount, payment.Amount.Amount);
        payment.BillingAddress = PropertyUpdater.UpdateIfChanged(req.BillingAddress, payment.BillingAddress);
        payment.Description = PropertyUpdater.UpdateIfChanged(req.Description, payment.Description);

        _tenantUow.Repository<Payment>().Update(payment);
        await _tenantUow.SaveChangesAsync();
        return Result.Ok();
    }

    private async Task<Result> TryUpdatePaymentMethod(Payment payment, Guid? paymentMethodId)
    {
        if (!paymentMethodId.HasValue || payment.MethodId == paymentMethodId)
        {
            return Result.Ok();
        }

        var paymentMethod = await _tenantUow.Repository<PaymentMethod>()
            .GetByIdAsync(paymentMethodId.Value);

        if (paymentMethod is null)
        {
            return Result.Fail($"Could not find a payment method with ID '{paymentMethodId}'");
        }

        payment.MethodId = paymentMethod.Id;
        return Result.Ok();
    }
}
