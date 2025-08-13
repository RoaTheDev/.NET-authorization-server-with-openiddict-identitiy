using Authorization.Application.Commands;
using Authorization.Application.Common;
using Authorization.Application.Dto.Response;
using Authorization.Application.interfaces;
using MediatR;

namespace Authorization.Application.Handler;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IAuthenticationService _authenticationService;

    public LoginCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authenticationService.LoginAsync(request, cancellationToken);
    }
}