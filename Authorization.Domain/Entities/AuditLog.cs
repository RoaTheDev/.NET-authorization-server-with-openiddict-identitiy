namespace Authorization.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SessionId { get; set; }
    public virtual User? User { get; set; }
}