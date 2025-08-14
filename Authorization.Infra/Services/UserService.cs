using System.Collections.Immutable;
using Authorization.Application.Commands;
using Authorization.Application.Common;
using Authorization.Application.Dto.Common;
using Authorization.Application.Dto.Request;
using Authorization.Application.Dto.Response;
using Authorization.Application.Extensions;
using Authorization.Application.interfaces;
using Authorization.Application.Queries;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Data;
using Authorization.Infrastructure.Mapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;
    private readonly AuthDbContext _dbContext;
    private readonly IAuditService _auditService;

    public UserService(UserManager<User> userManager, ICurrentUserService currentUserService, AuthDbContext dbContext,
        IAuditService auditService)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _auditService = auditService;
    }

    public async Task<Result<UserDto>> GetCurrentUser(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result<UserDto>.Failure("User not authenticated.");
        }

        var user = await _userManager.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure("User not found.");
        }

        var userDto = user.ToUserDto();

        ImmutableList<string> roles =
            user.UserRoles.Where(ur => ur.IsActive)
                .Select(ur => ur.Role.Name)
                .ToImmutableList()!;

        ImmutableList<string> permissions = user.UserRoles
            .Where(ur => ur.IsActive)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.FullPermission)
            .Distinct()
            .ToImmutableList();

        userDto = userDto with
        {
            Roles = roles,
            Permissions = permissions
        };

        return Result<UserDto>.Success(userDto);
    }

    public async Task<Result<PagedResult<UserDto>>> GetUsers(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(u =>
                u.FirstName.Contains(request.SearchTerm) ||
                u.LastName.Contains(request.SearchTerm) ||
                u.Email!.Contains(request.SearchTerm));
        }

        if (!string.IsNullOrEmpty(request.Role))
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == request.Role && ur.IsActive));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        var pagedResult = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        var usersDto = pagedResult.Items.Select(user =>
        {
            var dto = UserMapper.ToUserDto(user);
            var roles = user.UserRoles.Where(ur => ur.IsActive).Select(ur => ur.Role.Name).ToImmutableList();
            return dto with { Roles = roles! };
        }).ToList();

        var result = new PagedResult<UserDto>
        {
            Items = usersDto,
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };

        return Result<PagedResult<UserDto>>.Success(result);
    }

    public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result<bool>.Failure("User not authenticated");
        }

        var user = await _userManager.FindByIdAsync(_currentUserService.UserId.Value.ToString());
        if (user == null)
        {
            return Result<bool>.Failure("User not found.");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            await _auditService.LogAsync("ChangePassword", "User", false,
                $"Failed password change: {string.Join(", ", errors)}", userId: user.Id);
            return Result<bool>.Failure(errors);
        }

        await _auditService.LogAsync("ChangePassword", "User", true,
            "Password changed successfully", userId: user.Id);

        return Result<bool>.Success(true);
    }

    public async Task<Result<TwoFactorSetupDto>> EnableTwoFactorAsync(EnableTwoFactorCommand request,
        CancellationToken cancellation)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result<TwoFactorSetupDto>.Failure("User not authenticated");
        }

        var user = await _userManager.FindByIdAsync(_currentUserService.UserId.HasValue.ToString());
        if (user is null)
        {
            return Result<TwoFactorSetupDto>.Failure("User not found ");
        }

        await _userManager.ResetAuthenticatorKeyAsync(user);
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            return Result<TwoFactorSetupDto>.Failure("Failed to generate authenticator key.");
        }

        var qrCodeUrl = GenerateQrCodeUri(user.Email!, key);
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        await _auditService.LogAsync("EnableTwoFactor", "User", true,
            "Two-factor authentication setup initiated", userId: user.Id);

        return Result<TwoFactorSetupDto>.Success(new TwoFactorSetupDto
        {
            SecretKey = key,
            QrCodeUrl = qrCodeUrl,
            RecoveryCodes = recoveryCodes!.ToArray()
        });
    }

    private string GenerateQrCodeUri(string email, string key)
    {
        const string issuer = "AuthServer";
        return
            $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={key}&issuer={Uri.EscapeDataString(issuer)}";
    }
}