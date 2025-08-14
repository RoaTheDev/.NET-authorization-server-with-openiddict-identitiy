using Authorization.Application.Commands;
using FluentValidation;

namespace Authorization.Application.Validations;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password must not be empty")
            .NotNull().WithMessage("Password field must not be null")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password can only hold a maximum of 100 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$").WithMessage(
                "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("Password confirmation does not match.");
    }
}