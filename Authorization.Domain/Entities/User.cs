using System.Text;
using Authorization.Domain.interfaces;
using Microsoft.AspNetCore.Identity;

namespace Authorization.Domain.Entities;

public class User : IdentityUser<Guid>, IAggregateRoot
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public int LoginAttempts { get; set; }
    public DateTime? LockoutEndDateUtc { get; set; }
    public bool TwoFactorEnable { get; set; }
    public string? TwoFactorSecret { get; set; }
    public int LockoutCount { get; set; } = 0;
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();

    public string FullName => new StringBuilder($"{FirstName} {MiddleName} {LastName}").ToString();
    public bool IsLockedOut => LockoutEndDateUtc.HasValue && LockoutEndDateUtc > DateTime.UtcNow;

    public void IncrementLoginAttempts()
    {
        LoginAttempts++;
        if (LoginAttempts >= 5)
        {
            if (!LockoutEndDateUtc.HasValue || LockoutEndDateUtc <= DateTime.UtcNow)
            {
                LockoutCount++;
            }

            var lockoutDuration = TimeSpan.FromMinutes(5 * LockoutCount);
            LockoutEndDateUtc = DateTime.UtcNow.Add(lockoutDuration);
            LoginAttempts = 0;
        }
    }

    public void ResetLockout()
    {
        LockoutCount = 0;
        LoginAttempts = 0;
        LockoutEndDateUtc = null;
    }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    public void ClearDomainEvent() => _domainEvents.Clear();
}