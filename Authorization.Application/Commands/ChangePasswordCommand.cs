using Authorization.Application.Common;
using MediatR;

namespace Authorization.Application.Commands;

public record ChangePasswordCommand : IRequest<Result<bool>>
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
}
