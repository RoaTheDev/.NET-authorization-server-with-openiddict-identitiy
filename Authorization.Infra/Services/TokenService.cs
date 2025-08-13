using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Authorization.Application.Dto.Response;
using Authorization.Application.interfaces;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Configs;
using Authorization.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Authorization.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<User> _userManager;
    private readonly AuthDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly IDateTimeService _dateTimeService;

    public TokenService(UserManager<User> userManager, AuthDbContext context, IOptions<JwtSettings> jwtSettings,
        IDateTimeService dateTimeService)
    {
        _userManager = userManager;
        _context = context;
        _dateTimeService = dateTimeService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<TokenResponse> GenerateTokensAsync(User user, string? ipAddress = null, string? userAgent = null)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var customClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new("email_verified", user.EmailConfirmed.ToString().ToLower()),
            new("name", user.FullName),
        };

        customClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        customClaims.AddRange(claims);
        var accessToken = GenerateAccessToken(user, roles, customClaims);
        var refreshToken = await GenerateRefreshTokenAsync(user, accessToken, ipAddress, userAgent);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiredAt = _dateTimeService.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
        };
    }

    public async Task<RefreshResult> RefreshTokensAsync(string refreshToken, string? ipAddress = null,
        string? userAgent = null)
    {
        var storedRefreshToken = await _context.RefreshTokens.Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedRefreshToken is null || !storedRefreshToken.IsActive)
        {
            return new RefreshResult { IsValid = false };
        }

        storedRefreshToken.Use();
        await _context.SaveChangesAsync();
        var user = storedRefreshToken.User;
        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var customClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new("email_verified", user.EmailConfirmed.ToString().ToLower()),
            new("name", user.FullName),
        };

        customClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        customClaims.AddRange(claims);

        var newAccessToken = GenerateAccessToken(user, roles, customClaims);
        var newRefreshToken = await GenerateRefreshTokenAsync(user, newAccessToken, ipAddress, userAgent);

        return new RefreshResult
        {
            IsValid = true,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiredAt = _dateTimeService.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            UserId = user.Id
        };
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string reason)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (token is not null && token.IsActive)
        {
            token.Revoke(reason);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllUserRefreshTokenAsync(Guid userId, string reason)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsActive)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Revoke(reason);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        return token is { IsActive: true };
    }

    public string GenerateAccessToken(User user, IList<string> roles, IList<Claim> claims)
    {
        var jwtId = Guid.CreateVersion7().ToString();
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
        var allClaims = new List<Claim>(claims)
        {
            new("jti", jwtId),
            new("iat", new DateTimeOffset(_dateTimeService.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new("nbf", new DateTimeOffset(_dateTimeService.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new("exp", new DateTimeOffset(_dateTimeService.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes))
                .ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(allClaims),
            Expires = _dateTimeService.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            NotBefore = _dateTimeService.UtcNow
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(User user, string accessToken, string? ipAddress,
        string? userAgent)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(accessToken);
        var jwtId = jwt.Claims.FirstOrDefault(c => c.Type == "jti")?.Value ?? Guid.NewGuid().ToString();

        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[64];
        rng.GetBytes(randomBytes);

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            JwtId = jwtId,
            ExpiryDate = _dateTimeService.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            UserId = user.Id,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }
}