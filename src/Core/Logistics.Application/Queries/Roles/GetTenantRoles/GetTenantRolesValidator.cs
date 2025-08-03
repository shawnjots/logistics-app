﻿using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetTenantRolesValidator : AbstractValidator<GetTenantRolesQuery>
{
    public GetTenantRolesValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
