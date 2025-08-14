using Authorization.Infrastructure.Configs;
using Microsoft.AspNetCore.Authorization;

namespace Authorization.Infrastructure.Extensions;

public static class AuthorizationPolicyExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(this AuthorizationPolicyBuilder builder,
        string permission)
    {
        return builder.AddRequirements(new PermissionRequirement(permission));
    }
}