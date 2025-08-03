﻿using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetRolePermissionsHandler : 
    RequestHandler<GetRolePermissionsQuery, Result<PermissionDto[]>>
{
    private readonly IMasterUnityOfWork _masterUow;
    private readonly ITenantUnityOfWork _tenantUow;

    public GetRolePermissionsHandler(IMasterUnityOfWork masterUow, ITenantUnityOfWork tenantUow)
    {
        _masterUow = masterUow;
        _tenantUow = tenantUow;
    }

    protected override Task<Result<PermissionDto[]>> HandleValidated(
        GetRolePermissionsQuery req, CancellationToken cancellationToken)
    {
        if (req.RoleName.StartsWith("app"))
        {
            return GetAppRolePermissions(req);
        }
        if (req.RoleName.StartsWith("tenant"))
        {
            return GetTenantRolePermissions(req);
        }
        
        return Task.FromResult(Result<PermissionDto[]>.Fail($"Invalid role name '{req.RoleName}'"));
    }

    private async Task<Result<PermissionDto[]>> GetAppRolePermissions(GetRolePermissionsQuery req)
    {
        var role = await _masterUow.Repository<AppRole>().GetAsync(i => i.Name == req.RoleName);
        
        if (role is null)
        {
            return Result<PermissionDto[]>.Fail($"Could not find a role with name '{req.RoleName}'");
        }
        
        var permissions = await _masterUow.Repository<AppRoleClaim, int>().GetListAsync(i => i.RoleId == role.Id);
        var permissionsDto = permissions.Select(i => i.ToDto()).ToArray();
        return Result<PermissionDto[]>.Succeed(permissionsDto);
    }
    
    private async Task<Result<PermissionDto[]>> GetTenantRolePermissions(GetRolePermissionsQuery req)
    {
        var role = await _tenantUow.Repository<TenantRole>().GetAsync(i => i.Name == req.RoleName);
        
        if (role is null)
        {
            return Result<PermissionDto[]>.Fail($"Could not find a role with name '{req.RoleName}'");
        }
        
        var permissions = await _tenantUow.Repository<TenantRoleClaim>().GetListAsync(i => i.RoleId == role.Id);
        var permissionsDto = permissions.Select(i => i.ToDto()).ToArray();
        return Result<PermissionDto[]>.Succeed(permissionsDto);
    }
}
