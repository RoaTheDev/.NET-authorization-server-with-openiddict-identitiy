using Authorization.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : IdentityDbContext<User, Role, Guid, UserClaim, UserRole,
        IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>(options)
{
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>().ToTable("Users");
        builder.Entity<Role>().ToTable("Roles");
        builder.Entity<UserRole>().ToTable("UserRoles");
        builder.Entity<UserClaim>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
            entity.Property(e => e.TwoFactorSecret).HasMaxLength(100);

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();

            entity.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.AuditLogs)
                .WithOne(al => al.User)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        builder.Entity<RefreshToken>(entity =>
        {
            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
            entity.Property(e => e.JwtId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.RevokedBy).HasMaxLength(256);
            entity.Property(e => e.RevokedReason).HasMaxLength(500);

            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.JwtId);
            entity.HasIndex(e => new { e.UserId, e.IsRevoked, e.IsUsed });
        });
        builder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(256);
            entity.HasIndex(e => e.Name).IsUnique();
        });
        builder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            entity.Property(e => e.AssignedBy).HasMaxLength(256);

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        });
        builder.Entity<Permission>(entity =>
        {
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.Description).HasMaxLength(256);
            entity.Property(e => e.Resource).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();

            entity.HasIndex(e => new { e.Resource, e.Action }).IsUnique();
        });

        builder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            entity.Property(e => e.GrantedBy).HasMaxLength(256);

            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
            entity.Property(e => e.JwtId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.RevokedBy).HasMaxLength(256);
            entity.Property(e => e.RevokedReason).HasMaxLength(256);

            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.JwtId);
            entity.HasIndex(e => new { e.UserId, e.IsRevoked, e.IsUsed });
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Resource).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.SessionId).HasMaxLength(100);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TimeStamp);
            entity.HasIndex(e => new { e.Resource, e.Action });
        });

        builder.Entity<UserClaim>(entity => { entity.HasIndex(e => e.UserId); });

        SeedDefaultData(builder);
    }

    private static void SeedDefaultData(ModelBuilder builder)
    {
        var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Seed roles
        builder.Entity<Role>().HasData(
            new Role
            {
                Id = adminRoleId,
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR",
                Description = "Full system access",
                IsSystemRole = true,
                IsActive = true
            },
            new Role
            {
                Id = userRoleId,
                Name = "User",
                NormalizedName = "USER",
                Description = "Standard user access",
                IsSystemRole = false,
                IsActive = true
            }
        );

        // Seed permissions
        var permissions = new[]
        {
            new { Id = Guid.NewGuid(), Name = "Create User", Resource = "User", Action = "Create" },
            new { Id = Guid.NewGuid(), Name = "Read User", Resource = "User", Action = "Read" },
            new { Id = Guid.NewGuid(), Name = "Update User", Resource = "User", Action = "Update" },
            new { Id = Guid.NewGuid(), Name = "Delete User", Resource = "User", Action = "Delete" },
            new { Id = Guid.NewGuid(), Name = "Manage Roles", Resource = "Role", Action = "Manage" },
            new { Id = Guid.NewGuid(), Name = "View Dashboard", Resource = "Dashboard", Action = "View" }
        };

        builder.Entity<Permission>().HasData(
            permissions.Select(p => new Permission
            {
                Id = p.Id,
                Name = p.Name,
                Resource = p.Resource,
                Action = p.Action,
                IsActive = true
            })
        );

        // Assign all permissions to admin role
        builder.Entity<RolePermission>().HasData(
            permissions.Select(p => new RolePermission
            {
                RoleId = adminRoleId,
                PermissionId = p.Id,
                GrantedAt = DateTime.UtcNow
            })
        );

        // Assign view permissions to user role
        builder.Entity<RolePermission>().HasData(
            new RolePermission
            {
                RoleId = userRoleId,
                PermissionId = permissions.First(p => p.Action == "Read").Id,
                GrantedAt = DateTime.UtcNow
            },
            new RolePermission
            {
                RoleId = userRoleId,
                PermissionId = permissions.First(p => p.Action == "View").Id,
                GrantedAt = DateTime.UtcNow
            }
        );
    }
}