using Logistics.Shared.Models;

namespace Logistics.HttpClient.Models;

public record UpdateTenant
{
    public string? Id { get; set; }
    public string? CompanyName { get; set; }
    public AddressDto? CompanyAddress { get; set; }
    public string? Name {get; set;}
    
}
