﻿using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class SearchTenants : BaseSpecification<Tenant>
{
    public SearchTenants(
        string? search, 
        string? orderBy,
        int page,
        int pageSize)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = i =>
                (!string.IsNullOrEmpty(i.Name) &&
                 i.Name.Contains(search)) ||

                (!string.IsNullOrEmpty(i.CompanyName) &&
                 i.CompanyName.Contains(search));
        }
        
        ApplyOrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
    
    protected override Expression<Func<Tenant, object?>> CreateOrderByExpression(string propertyName)
    {
        return propertyName switch
        {
            "companyname" => i => i.CompanyName,
            "companyaddress" => i => i.CompanyAddress.Line1,
            "subscriptionplan" => i => i.Subscription!.Plan.Name,
            _ => i => i.Name
        };
    }
}
