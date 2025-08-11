using System.Text;

namespace Authorization.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public string FullPermission => $"{Resource}:{Action}";
}