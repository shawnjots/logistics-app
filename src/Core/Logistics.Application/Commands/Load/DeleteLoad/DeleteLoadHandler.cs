﻿using Logistics.Application.Extensions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared;

namespace Logistics.Application.Commands;

internal sealed class DeleteLoadHandler : RequestHandler<DeleteLoadCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IPushNotificationService _pushNotificationService;

    public DeleteLoadHandler(
        ITenantUnityOfWork tenantUow,
        IPushNotificationService pushNotificationService)
    {
        _tenantUow = tenantUow;
        _pushNotificationService = pushNotificationService;
    }

    protected override async Task<Result> HandleValidated(
        DeleteLoadCommand req, CancellationToken cancellationToken)
    {
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.Id);
        var truck = load?.AssignedTruck;
        
        _tenantUow.Repository<Load>().Delete(load);
        var changes = await _tenantUow.SaveChangesAsync();
        
        if (load is not null && changes > 0)
        {
            await _pushNotificationService.SendRemovedLoadNotificationAsync(load, truck);
        }
        
        return Result.Succeed();
    }
}
