using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateSubscriptionPlanHandler : IAppRequestHandler<CreateSubscriptionPlanCommand, Result>
{
    private readonly ILogger<CreateSubscriptionPlanHandler> _logger;
    private readonly IMasterUnitOfWork _masterUow;
    private readonly IStripeService _stripeService;

    public CreateSubscriptionPlanHandler(
        IMasterUnitOfWork masterUow,
        IStripeService stripeService,
        ILogger<CreateSubscriptionPlanHandler> logger)
    {
        _masterUow = masterUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        CreateSubscriptionPlanCommand req, CancellationToken ct)
    {
        var subscriptionPlan = new SubscriptionPlan
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            TrialPeriod = req.TrialPeriod,
            Interval = req.Interval,
            IntervalCount = req.IntervalCount
        };

        var (product, price) = await _stripeService.CreateSubscriptionPlanAsync(subscriptionPlan);

        subscriptionPlan.StripeProductId = product.Id;
        subscriptionPlan.StripePriceId = price.Id;
        await _masterUow.Repository<SubscriptionPlan>().AddAsync(subscriptionPlan);
        await _masterUow.SaveChangesAsync();
        return Result.Ok();
    }
}
