using Authorization.Application.Commands;
using Authorization.Application.Common;
using Authorization.Application.Dto.Response;
using Authorization.Application.interfaces;
using MediatR;

namespace Authorization.Application.Handler;

public class RefreshCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    private IAuthenticationService _authenticationService;

    public RefreshCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _authenticationService.RefreshAsync(request, cancellationToken);
    }
}