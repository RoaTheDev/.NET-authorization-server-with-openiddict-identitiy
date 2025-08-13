using Microsoft.AspNetCore.Identity;

namespace Authorization.Infrastructure.Identity;

public class ApplicationUserClaim : IdentityUserClaim<Guid>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}