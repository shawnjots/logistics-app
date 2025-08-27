using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetTripsQuery : PagedQuery, IAppRequest<PagedResult<TripDto>>
{
    public string? Name { get; set; }
    public TripStatus? Status { get; set; }
    public string? TruckNumber { get; set; }
}
