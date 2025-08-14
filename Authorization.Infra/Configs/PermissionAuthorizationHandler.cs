using System.Security.Claims;
using Authorization.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Configs;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly AuthDbContext _dbContext;

    public PermissionAuthorizationHandler(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return;
        }

        var hasPermission = await _dbContext.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoles)
            .Where(ur => ur.IsActive)
            .SelectMany(ur => ur.Role.RolePermissions)
            .AnyAsync(rp => rp.Permission.FullPermission == requirement.Permission && rp.Permission.IsActive);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

    }
}