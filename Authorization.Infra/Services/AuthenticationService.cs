using System.Collections.Immutable;
using Authorization.Application.Commands;
using Authorization.Application.Common;
using Authorization.Application.Dto.Response;
using Authorization.Application.interfaces;
using Authorization.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IAuditService _auditService;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;

    public AuthenticationService(UserManager<User> userManager, SignInManager<User> signInManager,
        IAuditService auditService, ITokenService tokenService, ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _auditService = auditService;
        _tokenService = tokenService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Where(u => u.Email == request.Email && u.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            await _auditService.LogAsync("Login", "Authentication", false,
                $"Failed login attempt for email: {request.Email}", request.IpAddress);
            return Result<LoginResponse>.Failure("Invalid credentials.");
        }

        if (user.IsLockedOut)
        {
            await _auditService.LogAsync("Login", "Authentication", false,
                $"Login attempt for locked user: {request.Email}", request.IpAddress);
            return Result<LoginResponse>.Failure("Account is locked. Please try again later.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
        if (!result.Succeeded)
        {
            user.IncrementLoginAttempts();
            await _userManager.UpdateAsync(user);

            await _auditService.LogAsync("Login", "Authentication", false,
                $"Invalid password for user: {request.Email}", request.IpAddress);
            return Result<LoginResponse>.Failure("Invalid credentials");
        }

        if (result.RequiresTwoFactor)
        {
            if (string.IsNullOrEmpty(request.TwoFactorCode))
            {
                return Result<LoginResponse>.Success(new LoginResponse
                {
                    RequiresTwoFactor = true
                });
            }

            var twoFactorResult =
                await _signInManager.TwoFactorAuthenticatorSignInAsync(request.TwoFactorCode, request.RememberMe,
                    false);
            if (!twoFactorResult.Succeeded)
            {
                user.UpdateLastLogin();
                await _auditService.LogAsync("Login", "Authentication", false,
                    $"Invalid 2FA code for user: {request.Email}", request.IpAddress);
                return Result<LoginResponse>.Failure("Invalid two-factor authentication code.");
            }

            user.ResetLockout();
            user.UpdateLastLogin();
            await _userManager.UpdateAsync(user);
        }

        var activeUserRoles = user.UserRoles.Where(ur => ur.IsActive);
        var userRoles = activeUserRoles.ToList();
        var roles = userRoles.Select(ur => ur.Role.Name).ToImmutableList();
        var permissions = userRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.FullPermission)
            .Distinct()
            .ToImmutableList();
        var token = await _tokenService.GenerateTokensAsync(user, request.IpAddress, request.UserAgent);
        await _auditService.LogAsync("Login", "Authentication", true,
            $"Successful login for user: {request.Email}", request.IpAddress, user.Id);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            ExpiredAt = token.ExpiredAt,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                Roles = roles!,
                Permissions = permissions
            }
        });
    }


    public async Task<Result<TokenResponse>> RefreshAsync(RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _tokenService.RefreshTokensAsync(request.RefreshToken, request.IpAddress, request.UserAgent);
        if (!result.IsValid)
        {
            await _auditService.LogAsync("RefreshToken", "Authentication", false,
                "Invalid refresh token attempt", request.IpAddress);
            return Result<TokenResponse>.Failure("Invalid Refresh Token");
        }

        await _auditService.LogAsync("RefreshToken", "Authentication", true,
            "Token refreshed successfully", request.IpAddress, result.UserId);
        return Result<TokenResponse>.Success(new TokenResponse
        {
            RefreshToken = result.RefreshToken,
            AccessToken = result.AccessToken,
            ExpiredAt = result.ExpiredAt
        });
    }

    public async Task<Result<bool>> LogoutAsync(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, "User logout");
        }
        else if (userId.HasValue)
        {
            await _tokenService.RevokeAllUserRefreshTokenAsync(userId.Value, "User logout all sessions");
        }

        await _auditService.LogAsync("Logout", "Authentication", true,
            "User logged out", request.IpAddress, userId);
        return Result<bool>.Success(true);
    }
}