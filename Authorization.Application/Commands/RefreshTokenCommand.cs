using Authorization.Application.Common;
using Authorization.Application.Dto.Response;
using MediatR;

namespace Authorization.Application.Commands;

public record RefreshTokenCommand : IRequest<Result<TokenResponse>>
{
    public string RefreshToken { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
};