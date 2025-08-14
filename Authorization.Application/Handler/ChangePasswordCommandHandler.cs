using Authorization.Application.Commands;
using Authorization.Application.Common;
using Authorization.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Authorization.Application.Handler;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}