using Authorization.Application.Common;
using MediatR;

namespace Authorization.Application.Commands;

public record LogoutCommand : IRequest<Result<bool>>
{
    public string? RefreshToken { get; init; }
    public string? IpAddress { get; init; }
}