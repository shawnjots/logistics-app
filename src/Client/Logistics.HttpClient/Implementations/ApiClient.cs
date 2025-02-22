﻿using System.IdentityModel.Tokens.Jwt;
using Logistics.Shared.Models;
using Logistics.HttpClient.Exceptions;
using Logistics.HttpClient.Options;

namespace Logistics.HttpClient.Implementations;

internal class ApiClient : GenericApiClient, IApiClient
{
    private string? _accessToken;
    private string? _tenantId;
    
    public ApiClient(ApiClientOptions options) : base(options.Host!)
    {
        AccessToken = options.AccessToken;
    }

    public event EventHandler<string>? OnErrorResponse;

    public string? AccessToken
    {
        get => _accessToken;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            _accessToken = value;
            SetAuthorizationHeader("Bearer", _accessToken);
            SetTenantIdFromAccessToken(_accessToken);
        }
    }

    public string? TenantId 
    {
        get => _tenantId;
        set
        {
            _tenantId = value;
            SetRequestHeader("X-Tenant", _tenantId);
        }
    }
    
    private void SetTenantIdFromAccessToken(string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            return;
        }
        
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(accessToken);
        var tenantId = token?.Claims?.FirstOrDefault(i => i.Type == "tenant")?.Value;
        
        if (string.IsNullOrEmpty(tenantId) || TenantId == tenantId)
        {
            return;
        } 

        TenantId = tenantId;
        SetRequestHeader("X-Tenant", tenantId);
    }

    private async Task<TRes> MakeGetRequestAsync<TRes>(string endpoint, IDictionary<string, string>? query = null)
        where TRes : class, IResult, new()
    {
        try
        {
            var result = await GetRequestAsync<TRes>(endpoint, query);

            if (!result.Success)
            {
                OnErrorResponse?.Invoke(this, result.Error!);
            }

            return result;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return new TRes { Error = ex.Message };
        }
    }

    private async Task<TRes> MakePostRequestAsync<TRes, TBody>(string endpoint, TBody body)
        where TRes : class, IResult, new()
        where TBody : class, new()
    {
        try
        {
            var result = await PostRequestAsync<TRes, TBody>(endpoint, body);

            if (!result.Success)
            {
                OnErrorResponse?.Invoke(this, result.Error!);
            }

            return result;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return new TRes { Error = ex.Message };
        }
    }

    private async Task<TRes> MakePutRequestAsync<TRes, TBody>(string endpoint, TBody body)
        where TRes : class, IResult, new()
        where TBody : class, new()
    {
        try
        {
            var result = await PutRequestAsync<TRes, TBody>(endpoint, body);

            if (!result.Success)
            {
                OnErrorResponse?.Invoke(this, result.Error!);
            }

            return result;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return new TRes { Error = ex.Message };
        }
    }

    private async Task<TRes> MakeDeleteRequestAsync<TRes>(string endpoint)
        where TRes : class, IResult, new()
    {
        try
        {
            var result = await DeleteRequestAsync<TRes>(endpoint);

            if (!result.Success)
            {
                OnErrorResponse?.Invoke(this, result.Error!);
            }

            return result;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return new TRes { Error = ex.Message };
        }
    }


    #region Load API
    
    public Task<Result<LoadDto>> GetLoadAsync(string id)
    {
        return MakeGetRequestAsync<Result<LoadDto>>($"loads/{id}");
    }

    public Task<PagedResult<LoadDto>> GetLoadsAsync(GetLoadsQuery query)
    {
        return MakeGetRequestAsync<PagedResult<LoadDto>>("loads", query.ToDictionary());
    }
    
    public Task<Result<ICollection<LoadDto>>> GetDriverActiveLoadsAsync(string userId)
    {
        var query = new Dictionary<string, string>
        {
            { "userId", userId },
            { "onlyActiveLoads", "true" },
            { "loadAllPage", "true" }
        };
        return MakeGetRequestAsync<Result<ICollection<LoadDto>>>("loads", query);
    }

    public Task<Result> CreateLoadAsync(CreateLoad command)
    {
        return MakePostRequestAsync<Result, CreateLoad>("loads", command);
    }
    
    public Task<Result> UpdateLoadAsync(UpdateLoad command)
    {
        return MakePutRequestAsync<Result, UpdateLoad>($"loads/{command.Id}", command);
    }

    public Task<Result> DeleteLoadAsync(string id)
    {
        return MakeDeleteRequestAsync<Result>($"loads/{id}");
    }

    #endregion


    #region Truck API

    public Task<Result<TruckDto>> GetTruckAsync(GetTruckQuery query)
    {
        var id = query.TruckOrDriverId;
        return MakeGetRequestAsync<Result<TruckDto>>($"trucks/{id}", query.ToDictionary());
    }

    public Task<PagedResult<TruckDto>> GetTrucksAsync(SearchableQuery query, bool includeLoads = false)
    {
        var queryDict = query.ToDictionary();
        queryDict.Add("includeLoads", includeLoads.ToString());
        return MakeGetRequestAsync<PagedResult<TruckDto>>("trucks", queryDict);
    }

    public Task<Result> CreateTruckAsync(CreateTruck command)
    {
        return MakePostRequestAsync<Result, CreateTruck>("trucks", command);
    }

    public Task<Result> UpdateTruckAsync(UpdateTruck command)
    {
        return MakePutRequestAsync<Result, UpdateTruck>($"trucks/{command.Id}", command);
    }

    public Task<Result> DeleteTruckAsync(string id)
    {
        return MakeDeleteRequestAsync<Result>($"trucks/{id}");
    }

    #endregion


    #region Employee API

    public Task<Result<EmployeeDto>> GetEmployeeAsync(string userId)
    {
        return MakeGetRequestAsync<Result<EmployeeDto>>($"employees/{userId}");
    }

    public Task<PagedResult<EmployeeDto>> GetEmployeesAsync(SearchableQuery query)
    {
        return MakeGetRequestAsync<PagedResult<EmployeeDto>>("employees", query.ToDictionary());
    }

    public Task<Result> CreateEmployeeAsync(CreateEmployee command)
    {
        return MakePostRequestAsync<Result, CreateEmployee>("employees", command);
    }

    public Task<Result> UpdateEmployeeAsync(UpdateEmployee command)
    {
        return MakePutRequestAsync<Result, UpdateEmployee>($"employees/{command.UserId}", command);
    }
    
    public Task<Result> DeleteEmployeeAsync(string userId)
    {
        return MakeDeleteRequestAsync<Result>($"employees/{userId}");
    }

    #endregion


    #region Tenant API

    public Task<Result<TenantDto>> GetTenantAsync(string identifier)
    {
        return MakeGetRequestAsync<Result<TenantDto>>($"tenants/{identifier}");
    }

    public Task<PagedResult<TenantDto>> GetTenantsAsync(SearchableQuery query)
    {
        return MakeGetRequestAsync<PagedResult<TenantDto>>("tenants", query.ToDictionary());
    }

    public Task<Result> CreateTenantAsync(CreateTenant command)
    {
        return MakePostRequestAsync<Result, CreateTenant>("tenants", command);
    }

    public Task<Result> UpdateTenantAsync(UpdateTenant command)
    {
        return MakePutRequestAsync<Result, UpdateTenant>($"tenants/{command.Id}", command);
    }

    public Task<Result> DeleteTenantAsync(string id)
    {
        return MakeDeleteRequestAsync<Result>($"tenants/{id}");
    }

    #endregion


    #region User API

    public Task<Result<UserDto>> GetUserAsync(string userId)
    {
        return MakeGetRequestAsync<Result<UserDto>>($"users/{userId}");
    }

    public Task<PagedResult<UserDto>> GetUsersAsync(SearchableQuery query)
    {
        return MakeGetRequestAsync<PagedResult<UserDto>>("users", query.ToDictionary());
    }

    public Task<Result> UpdateUserAsync(UpdateUser command)
    {
        return MakePutRequestAsync<Result, UpdateUser>($"users/{command.UserId}", command);
    }

    public Task<Result<OrganizationDto[]>> GetUserOrganizations(string userId)
    {
        return MakeGetRequestAsync<Result<OrganizationDto[]>>($"users/{userId}/organizations");
    }

    #endregion


    #region Driver API
    
    public Task<Result> SetDeviceTokenAsync(SetDeviceToken command)
    {
        return MakePostRequestAsync<Result, SetDeviceToken>($"drivers/{command.UserId}/device-token", command);
    }

    public Task<Result> ConfirmLoadStatusAsync(ConfirmLoadStatus command)
    {
        return MakePostRequestAsync<Result, ConfirmLoadStatus>("drivers/confirm-load-status", command);
    }

    public Task<Result> UpdateLoadProximity(UpdateLoadProximity command)
    {
        return MakePostRequestAsync<Result, UpdateLoadProximity>("drivers/update-load-proximity", command);
    }

    #endregion


    #region Stats API

    public Task<Result<DailyGrossesDto>> GetDailyGrossesAsync(GetDailyGrossesQuery query)
    {
        return MakeGetRequestAsync<Result<DailyGrossesDto>>("stats/daily-grosses", query.ToDictionary());
    }

    public Task<Result<MonthlyGrossesDto>> GetMonthlyGrossesAsync(GetMonthlyGrossesQuery query)
    {
        return MakeGetRequestAsync<Result<MonthlyGrossesDto>>("stats/monthly-grosses", query.ToDictionary());
    }

    public Task<Result<DriverStatsDto>> GetDriverStatsAsync(string userId)
    {
        return MakeGetRequestAsync<Result<DriverStatsDto>>($"stats/driver/{userId}");  
    }

    #endregion

    
    #region Subscriptions API

    public Task<Result<SubscriptionDto>> GetSubscriptionAsync(string id)
    {
        return MakeGetRequestAsync<Result<SubscriptionDto>>($"subscriptions/{id}");
    }

    public Task<Result<SubscriptionPlanDto>> GetSubscriptionPlanAsync(string planId)
    {
        return MakeGetRequestAsync<Result<SubscriptionPlanDto>>($"subscriptions/plans/{planId}");
    }

    public Task<PagedResult<SubscriptionDto>> GetSubscriptionsAsync(PagedQuery query)
    {
        return MakeGetRequestAsync<PagedResult<SubscriptionDto>>("subscriptions", query.ToDictionary());
    }

    public Task<PagedResult<SubscriptionPlanDto>> GetSubscriptionPlansAsync(PagedQuery query)
    {
        return MakeGetRequestAsync<PagedResult<SubscriptionPlanDto>>("subscriptions/plans", query.ToDictionary());
    }

    public Task<Result> CreateSubscriptionPlanAsync(CreateSubscriptionPlan command)
    {
        return MakePostRequestAsync<Result, CreateSubscriptionPlan>("subscriptions/plans", command);
    }

    public Task<Result> UpdateSubscriptionPlanAsync(UpdateSubscriptionPlan command)
    {
        return MakePutRequestAsync<Result, UpdateSubscriptionPlan>($"subscriptions/plans/{command.Id}", command);
    }

    public Task<Result> DeleteSubscriptionPlanAsync(string id)
    {
        return MakeDeleteRequestAsync<Result>($"subscriptions/plans/{id}");
    }

    public Task<Result> CreateSubscriptionAsync(CreateSubscription command)
    {
        return MakePostRequestAsync<Result, CreateSubscription>("subscriptions", command);
    }

    public Task<Result> UpdateSubscriptionAsync(UpdateSubscription command)
    {
        return MakePutRequestAsync<Result, UpdateSubscription>($"subscriptions/{command.Id}", command);
    }

    public Task<Result> DeleteSubscriptionAsync(string id)
    {
        return MakeDeleteRequestAsync<Result>($"subscriptions/{id}");
    }

    #endregion
}
