﻿using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared;

namespace Logistics.Application.Commands;

internal sealed class ConfirmLoadStatusHandler : RequestHandler<ConfirmLoadStatusCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly INotificationService _notificationService;

    public ConfirmLoadStatusHandler(
        ITenantUnityOfWork tenantUow,
        INotificationService notificationService)
    {
        _tenantUow = tenantUow;
        _notificationService = notificationService;
    }

    protected override async Task<Result> HandleValidated(
        ConfirmLoadStatusCommand req, CancellationToken cancellationToken)
    {
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.LoadId);

        if (load is null)
        {
            return Result.Fail($"Could not find load with ID '{req.LoadId}'");
        }

        var loadStatus = req.LoadStatus!.Value;
        load.SetStatus(loadStatus);
        
        _tenantUow.Repository<Load>().Update(load);
        var changes = await _tenantUow.SaveChangesAsync();

        if (changes > 0)
        {
            await SendNotificationAsync(load, req.DriverId!);
        }
        
        return Result.Succeed();
    }

    private async Task SendNotificationAsync(Load load, string driverId)
    {
        const string title = "Load updates";
        var driverName = load.AssignedTruck?.Drivers.FirstOrDefault(i => i.Id == driverId)?.GetFullName();
        var message = $"Driver {driverName} confirmed the load #{load.RefId} status to '{load.GetStatus()}'";
        await _notificationService.SendNotificationAsync(title, message);
    }
}
