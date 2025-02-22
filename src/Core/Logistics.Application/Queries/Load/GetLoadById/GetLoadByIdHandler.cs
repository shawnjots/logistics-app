﻿using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetLoadByIdHandler : RequestHandler<GetLoadByIdQuery, Result<LoadDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetLoadByIdHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result<LoadDto>> HandleValidated(
        GetLoadByIdQuery req, CancellationToken cancellationToken)
    {
        var loadEntity = await _tenantUow.Repository<Load>().GetByIdAsync(req.Id);

        if (loadEntity is null)
        {
            return Result<LoadDto>.Fail($"Could not find a load with ID '{req.Id}'");
        }
        
        var loadDto = loadEntity.ToDto();
        return Result<LoadDto>.Succeed(loadDto);
    }
}
