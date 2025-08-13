using Authorization.Domain.Entities;
using Authorization.Infrastructure.Identity;

namespace Authorization.Infrastructure.Mapper;

public static class RoleMapper
{
    public static ApplicationRole ToApplicationRole(this Role role)
    {
        return new ApplicationRole
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            CreatedAt = role.CreatedAt,
            IsActive = role.IsActive
        };
    }

    public static Role ToDomainRole(this ApplicationRole appRole)
    {
        return new Role
        {
            Id = appRole.Id,
            Name = appRole.Name!,
            Description = appRole.Description,
            IsSystemRole = appRole.IsSystemRole,
            CreatedAt = appRole.CreatedAt,
            IsActive = appRole.IsActive
        };
    }
}