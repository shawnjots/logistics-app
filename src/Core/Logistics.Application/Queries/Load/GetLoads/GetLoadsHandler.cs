﻿using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetLoadsHandler : RequestHandler<GetLoadsQuery, PagedResult<LoadDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetLoadsHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResult<LoadDto>> HandleValidated(
        GetLoadsQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = await _tenantUow.Repository<Load>().CountAsync();
        var spec = new SearchLoads(req.Search, req.OrderBy);

        var baseQuery = _tenantUow.Repository<Load>().ApplySpecification(spec);

        if (req.OnlyActiveLoads)
        {
            baseQuery = baseQuery.Where(i => i.DeliveryDate == null);
        }
        if (!string.IsNullOrEmpty(req.UserId))
        {
            baseQuery = baseQuery.Where(i => i.AssignedTruck != null &&
                                             i.AssignedTruck.Drivers.Select(emp => emp.Id).Contains(req.UserId));
        }
        if (!string.IsNullOrEmpty(req.TruckId))
        {
            baseQuery = baseQuery.Where(i => i.AssignedTruckId == req.TruckId);
        }
        if (req is { StartDate: not null, EndDate: not null })
        {
            baseQuery = baseQuery.Where(i => i.DispatchedDate >= req.StartDate && 
                                             i.DispatchedDate <= req.EndDate);
        }
        if (!req.LoadAllPages)
        {
            baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);
        }
        
        var loads = baseQuery.Select(i => i.ToDto()).ToArray();
        return PagedResult<LoadDto>.Succeed(loads, totalItems, req.PageSize);
    }
}
