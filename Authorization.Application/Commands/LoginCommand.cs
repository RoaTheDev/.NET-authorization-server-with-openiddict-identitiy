using Authorization.Application.Common;
using Authorization.Application.Dto.Response;
using MediatR;

namespace Authorization.Application.Commands;

public record LoginCommand : IRequest<Result<LoginResponse>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool RememberMe { get; init; }
    public string? TwoFactorCode { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}

