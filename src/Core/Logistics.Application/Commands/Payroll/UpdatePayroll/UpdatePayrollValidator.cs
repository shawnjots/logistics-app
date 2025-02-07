﻿using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdatePayrollValidator : AbstractValidator<UpdatePayrollCommand>
{
    public UpdatePayrollValidator()
    {
        RuleFor(i => i.StartDate).LessThan(i => i.EndDate);
        RuleFor(i => i.Id).NotEmpty();
    }
}
