using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record UpdateTenantCommand
{
    public Guid? Id { get; set; }
    public string? CompanyName { get; set; }
    public Address? CompanyAddress { get; set; }
    public string? Name { get; set; }
}
