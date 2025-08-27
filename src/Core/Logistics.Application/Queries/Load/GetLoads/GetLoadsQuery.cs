using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetLoadsQuery : SearchableQuery, IAppRequest<PagedResult<LoadDto>>
{
    public bool LoadAllPages { get; set; }
    public bool OnlyActiveLoads { get; set; }
    public Guid? UserId { get; set; }
    public Guid? TruckId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
