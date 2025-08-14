using Authorization.Application.Common;
using Authorization.Application.Dto.Response;
using MediatR;

namespace Authorization.Application.Commands;

public record GetCurrentUserQuery : IRequest<Result<UserDto>>;
