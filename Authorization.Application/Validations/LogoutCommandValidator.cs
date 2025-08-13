using Authorization.Application.Commands;
using FluentValidation;

namespace Authorization.Application.Validations;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token must not be null when inputted.")
            .When(x => !string.IsNullOrEmpty(x.RefreshToken));
    }
}