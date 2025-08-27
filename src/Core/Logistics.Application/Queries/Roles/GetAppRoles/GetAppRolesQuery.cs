using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetAppRolesQuery : SearchableQuery, IAppRequest<PagedResult<RoleDto>>
{
}
