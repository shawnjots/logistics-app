﻿using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetEmployeesHandler : RequestHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetEmployeesHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResult<EmployeeDto>> HandleValidated(
        GetEmployeesQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = await _tenantUow.Repository<Employee>().CountAsync();
        var employeesQuery = _tenantUow.Repository<Employee>().Query();
        var specification = new SearchEmployees(req.Search, req.OrderBy, req.Page, req.PageSize);

        if (!string.IsNullOrEmpty(req.Role))
        {
            var role = await _tenantUow.Repository<TenantRole>().GetAsync(i => i.Name.Contains(req.Role));
            if (role is not null)
            {
                employeesQuery = _tenantUow.Repository<EmployeeTenantRole>()
                    .Query()
                    .Where(i => i.RoleId == role.Id)
                    .Select(i => i.Employee);
            }
        }

        var employeeDto = employeesQuery.ApplySpecification(specification)
            .Select(employeeEntity => employeeEntity.ToDto())
            .ToArray();
        
        return PagedResult<EmployeeDto>.Succeed(employeeDto, totalItems, req.PageSize);
    }
}
