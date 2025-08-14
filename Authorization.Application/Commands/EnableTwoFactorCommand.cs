using Authorization.Application.Common;
using Authorization.Application.Dto.Request;
using MediatR;

namespace Authorization.Application.Commands;

public class EnableTwoFactorCommand : IRequest<Result<TwoFactorSetupDto>>;

