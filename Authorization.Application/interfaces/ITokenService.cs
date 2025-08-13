using System.Security.Claims;
using Authorization.Application.Dto.Response;
using Authorization.Domain.Entities;

namespace Authorization.Application.interfaces;

public interface ITokenService
{
    Task<TokenResponse> GenerateTokensAsync(User user, string? ipAddress = null, string? userAgent = null);
    Task<RefreshResult> RefreshTokensAsync(string refreshToken, string? ipAddress = null, string? userAgent = null);
    Task RevokeRefreshTokenAsync(string refreshToken, string reason);
    Task RevokeAllUserRefreshTokenAsync(Guid userId, string refreshToken);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken);
    string GenerateAccessToken(User user, IList<Role> roles, IList<Claim> claims);
}