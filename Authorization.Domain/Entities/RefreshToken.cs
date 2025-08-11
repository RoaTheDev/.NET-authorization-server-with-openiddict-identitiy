namespace Authorization.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public string? RevokedBy { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;

    public bool IsActive => !IsRevoked && !IsUsed && !IsExpired;

    public void Revoke(string reason, string? revokeBy = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
        RevokedBy = revokeBy;
    }

    public void Use() => IsUsed = true;
}