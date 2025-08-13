using Microsoft.AspNetCore.Identity;

namespace Authorization.Infrastructure.Identity;

public class ApplicationUserRole : IdentityUserRole<Guid>
{
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? AssignedBy { get; set; }
    public DateTime ExpiredAt { get; set; }
    public bool IsActive { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ApplicationRole Role { get; set; } = null!;
}