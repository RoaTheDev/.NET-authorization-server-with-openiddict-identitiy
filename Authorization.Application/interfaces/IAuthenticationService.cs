using Authorization.Application.Commands;
using Authorization.Application.Common;
using Authorization.Application.Dto.Common;
using Authorization.Application.Dto.Response;
using Authorization.Application.Queries;

namespace Authorization.Application.interfaces;

public interface IAuthenticationService
{
    Task<Result<LoginResponse>> LoginAsync(LoginCommand request, CancellationToken cancellationToken);
    Task<Result<TokenResponse>> RefreshAsync(RefreshTokenCommand request, CancellationToken cancellationToken);
    Task<Result<bool>> LogoutAsync(LogoutCommand request, CancellationToken cancellationToken);
}