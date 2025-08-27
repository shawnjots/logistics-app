using Logistics.HttpClient.Abstractions;

namespace Logistics.HttpClient;

public interface IApiClient :
    ILoadApi,
    ITruckApi,
    IEmployeeApi,
    ITenantApi,
    IUserApi,
    IDriverApi,
    IStatsApi,
    ISubscriptionApi
{
    string? AccessToken { get; set; }
    Guid? TenantId { get; set; }
    public event EventHandler<string>? OnErrorResponse;
}
