using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class CreateTruckCommand : IAppRequest
{
    public string TruckNumber { get; set; } = null!;
    public TruckType TruckType { get; set; }
    public Guid MainDriverId { get; set; }
}
