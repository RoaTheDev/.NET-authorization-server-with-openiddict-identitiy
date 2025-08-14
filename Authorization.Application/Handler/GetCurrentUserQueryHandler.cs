using Authorization.Application.Commands;
using Authorization.Application.Common;
using Authorization.Application.Dto.Response;
using Authorization.Application.interfaces;
using MediatR;

namespace Authorization.Application.Handler;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly IUserService _userService;

    public GetCurrentUserQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        return await _userService.GetCurrentUser(request, cancellationToken);
    }
}