﻿using FluentValidation;
using Logistics.Shared;

namespace Logistics.Application.Commands;

internal sealed class CreateSubscriptionPlanValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanValidator()
    {
        RuleFor(i => i.Name).NotEmpty();
        RuleFor(i => i.Price).GreaterThanOrEqualTo(0);
    }
}
