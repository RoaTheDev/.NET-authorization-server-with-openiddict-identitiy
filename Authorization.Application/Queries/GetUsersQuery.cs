using Authorization.Application.Common;
using Authorization.Application.Dto.Common;
using Authorization.Application.Dto.Response;
using MediatR;

namespace Authorization.Application.Queries;

public record GetUsersQuery : IRequest<Result<PagedResult<UserDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? Role { get; init; }
    public bool? IsActive { get; init; }
}