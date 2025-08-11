using Microsoft.AspNetCore.Identity;

namespace Authorization.Domain.Entities;

public class UserRole : IdentityUserRole<Guid>
{
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? AssignedBy { get; set; }
    public DateTime ExpiredAt { get; set; }
    public bool IsActive { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}