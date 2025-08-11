using Microsoft.AspNetCore.Identity;

namespace Authorization.Domain.Entities;

public class UserClaim : IdentityUserClaim<Guid>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}