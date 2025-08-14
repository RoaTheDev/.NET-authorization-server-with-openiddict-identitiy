using Authorization.Application.Commands;
using Authorization.Application.Common;
using Authorization.Application.Dto.Common;
using Authorization.Application.Dto.Request;
using Authorization.Application.Dto.Response;
using Authorization.Application.Queries;

namespace Authorization.Application.interfaces;

public interface IUserService
{
    Task<Result<UserDto>> GetCurrentUser(GetCurrentUserQuery request, CancellationToken cancellationToken);
    Task<Result<PagedResult<UserDto>>> GetUsers(GetUsersQuery request, CancellationToken cancellationToken);

    Task<Result<bool>> ChangePasswordAsync(ChangePasswordCommand request, CancellationToken cancellationToken);

    Task<Result<TwoFactorSetupDto>>
        EnableTwoFactorAsync(EnableTwoFactorCommand request, CancellationToken cancellation);
    
}