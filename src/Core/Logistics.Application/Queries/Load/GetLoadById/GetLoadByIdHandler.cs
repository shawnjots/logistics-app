using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetLoadByIdHandler : IAppRequestHandler<GetLoadByIdQuery, Result<LoadDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetLoadByIdHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result<LoadDto>> Handle(
        GetLoadByIdQuery req, CancellationToken ct)
    {
        var loadEntity = await _tenantUow.Repository<Load>().GetByIdAsync(req.Id);

        if (loadEntity is null)
        {
            return Result<LoadDto>.Fail($"Could not find a load with ID '{req.Id}'");
        }

        var loadDto = loadEntity.ToDto();
        return Result<LoadDto>.Ok(loadDto);
    }
}
