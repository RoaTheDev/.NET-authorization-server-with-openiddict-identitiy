using Authorization.Application.Common;
using Authorization.Application.Dto.Common;
using Authorization.Application.Dto.Response;
using Authorization.Application.interfaces;
using Authorization.Application.Queries;
using MediatR;

namespace Authorization.Application.Handler;

public class GetUserQueryHandler : IRequestHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
{
    private readonly IUserService _userService;

    public GetUserQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _userService.GetUsers(request, cancellationToken);
    }
}