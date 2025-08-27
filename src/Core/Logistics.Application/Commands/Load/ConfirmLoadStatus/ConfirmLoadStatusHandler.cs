using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class ConfirmLoadStatusHandler : IAppRequestHandler<ConfirmLoadStatusCommand, Result>
{
    private readonly INotificationService _notificationService;
    private readonly ITenantUnitOfWork _tenantUow;

    public ConfirmLoadStatusHandler(
        ITenantUnitOfWork tenantUow,
        INotificationService notificationService)
    {
        _tenantUow = tenantUow;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(ConfirmLoadStatusCommand req, CancellationToken ct)
    {
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);

        if (load is null)
        {
            return Result.Fail($"Could not find load with ID '{req.LoadId}'");
        }

        var loadStatus = req.LoadStatus!.Value;
        load.UpdateStatus(loadStatus, true);

        var changes = await _tenantUow.SaveChangesAsync(ct);

        if (changes > 0)
        {
            await SendNotificationAsync(load);
        }

        return Result.Ok();
    }

    private async Task SendNotificationAsync(Load load)
    {
        const string title = "Load updates";
        var driverName = load.AssignedTruck?.MainDriver?.GetFullName();
        var message = $"Driver {driverName} confirmed the load #{load.Number} status to '{load.Status}'";
        await _notificationService.SendNotificationAsync(title, message);
    }
}
