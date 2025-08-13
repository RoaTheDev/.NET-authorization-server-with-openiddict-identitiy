using Authorization.Application.Commands;
using FluentValidation;

namespace Authorization.Application.Validations;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token must not be empty")
            .MaximumLength(256).WithMessage("The Refresh token max length is 256");
    }
}