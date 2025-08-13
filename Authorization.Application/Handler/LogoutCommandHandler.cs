using Authorization.Application.Commands;
using Authorization.Application.Common;
using Authorization.Application.interfaces;
using MediatR;

namespace Authorization.Application.Handler;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IAuthenticationService _authenticationService;

    public LogoutCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return await _authenticationService.LogoutAsync(request, cancellationToken);
    }
}