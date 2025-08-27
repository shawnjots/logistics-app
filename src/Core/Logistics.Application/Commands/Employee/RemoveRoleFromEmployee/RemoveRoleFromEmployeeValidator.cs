using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class RemoveRoleFromEmployeeValidator : AbstractValidator<RemoveRoleFromEmployeeCommand>
{
    public RemoveRoleFromEmployeeValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
        RuleFor(i => i.Role).NotEmpty();
    }
}
