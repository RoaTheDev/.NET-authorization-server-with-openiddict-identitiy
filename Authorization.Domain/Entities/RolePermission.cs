namespace Authorization.Domain.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public string? GrantedBy { get; set; }
    
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}