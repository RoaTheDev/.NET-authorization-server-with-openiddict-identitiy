using Authorization.Application.Commands;
using Authorization.Application.Common;
using Authorization.Application.Dto.Request;
using MediatR;

namespace Authorization.Application.Handler;

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand,Result<TwoFactorSetupDto>>
{
    
    public Task<Result<TwoFactorSetupDto>> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}