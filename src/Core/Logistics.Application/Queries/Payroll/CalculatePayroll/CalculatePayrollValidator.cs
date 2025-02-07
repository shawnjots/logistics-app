﻿using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class CalculatePayrollValidator : AbstractValidator<CalculatePayrollQuery>
{
    public CalculatePayrollValidator()
    {
        RuleFor(i => i.StartDate).LessThan(i => i.EndDate);
        RuleFor(i => i.EmployeeId).NotEmpty();
    }
}
