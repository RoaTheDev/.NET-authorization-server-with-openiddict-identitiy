using Authorization.Domain.Entities;
using Authorization.Infrastructure.Identity;

namespace Authorization.Infrastructure.Mapper;

public static class UserClaimMapper
{
    public static ApplicationUserClaim ToApplicationUserClaim(this UserClaim claim)
    {
        return new ApplicationUserClaim
        {
            Id = claim.Id,
            UserId = claim.UserId,
            ClaimType = claim.ClaimType,
            ClaimValue = claim.ClaimValue,
            CreatedAt = claim.CreatedAt,
            ExpiresAt = claim.ExpiresAt,
            IsActive = claim.IsActive
        };
    }

    public static UserClaim ToDomainUserClaim(this ApplicationUserClaim appClaim)
    {
        return new UserClaim
        {
            Id = appClaim.Id,
            UserId = appClaim.UserId,
            ClaimType = appClaim.ClaimType!,
            ClaimValue = appClaim.ClaimValue!,
            CreatedAt = appClaim.CreatedAt,
            ExpiresAt = appClaim.ExpiresAt,
            IsActive = appClaim.IsActive
        };
    }
}