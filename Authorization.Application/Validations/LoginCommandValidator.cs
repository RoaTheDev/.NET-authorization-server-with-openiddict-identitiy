using Authorization.Application.Commands;
using FluentValidation;

namespace Authorization.Application.Validations;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email should not be empty")
            .NotNull().WithMessage("Email field should not be given null")
            .EmailAddress().WithMessage("Must be a valid Email address")
            .MaximumLength(256).WithMessage("Maximum is 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password must not be empty")
            .NotNull().WithMessage("Password field must not be null")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password can only hold a maximum of 100 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$").WithMessage(
                "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

        RuleFor(x => x.TwoFactorCode)
            .Length(6)
            .When(x => !string.IsNullOrEmpty(x.TwoFactorCode));
    }
}