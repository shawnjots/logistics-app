using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetAppRolesHandler : IAppRequestHandler<GetAppRolesQuery, PagedResult<RoleDto>>
{
    private readonly IMasterUnitOfWork _masterUow;

    public GetAppRolesHandler(IMasterUnitOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    public async Task<PagedResult<RoleDto>> Handle(
        GetAppRolesQuery req, CancellationToken ct)
    {
        var totalItems = await _masterUow.Repository<AppRole>().CountAsync();

        var rolesDto = _masterUow.Repository<AppRole>()
            .ApplySpecification(new SearchAppRoles(req.Search, req.Page, req.PageSize))
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<RoleDto>.Succeed(rolesDto, totalItems, req.PageSize);
    }
}
