using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteSubscriptionPlanHandler : IAppRequestHandler<DeleteSubscriptionPlanCommand, Result>
{
    private readonly IMasterUnitOfWork _masterUow;

    public DeleteSubscriptionPlanHandler(IMasterUnitOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    public async Task<Result> Handle(
        DeleteSubscriptionPlanCommand req, CancellationToken ct)
    {
        var subscriptionPlan = await _masterUow.Repository<SubscriptionPlan>().GetByIdAsync(req.Id);

        if (subscriptionPlan is null)
        {
            return Result.Fail($"Could not find a subscription plan with ID '{req.Id}'");
        }

        _masterUow.Repository<SubscriptionPlan>().Delete(subscriptionPlan);
        await _masterUow.SaveChangesAsync();
        return Result.Ok();
    }
}
