using Authorization.Domain.Entities;
using Authorization.Infrastructure.Identity;

namespace Authorization.Infrastructure.Mapper;

public static class UserMapper
{
    public static ApplicationUser ToApplicationUser(this User user)
    {
        return new ApplicationUser
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            LoginAttempts = user.LoginAttempts,
            LockoutEndDateUtc = user.LockoutEndDateUtc,
            TwoFactorEnable = user.TwoFactorEnable,
            TwoFactorSecret = user.TwoFactorSecret,
            LockoutCount = user.LockoutCount
        };
    }

    public static User ToDomainUser(this ApplicationUser appUser)
    {
        return new User
        {
            Id = appUser.Id,
            Email = appUser.Email!,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            MiddleName = appUser.MiddleName,
            DateOfBirth = appUser.DateOfBirth,
            IsActive = appUser.IsActive,
            LastLoginAt = appUser.LastLoginAt,
            LoginAttempts = appUser.LoginAttempts,
            LockoutEndDateUtc = appUser.LockoutEndDateUtc,
            TwoFactorEnable = appUser.TwoFactorEnable,
            TwoFactorSecret = appUser.TwoFactorSecret,
            LockoutCount = appUser.LockoutCount
        };
    }
}