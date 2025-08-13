using System.Collections.Immutable;

namespace Authorization.Application.Dto.Response;

public record LoginResponse
{
    public string? AccessToken { get; init; } = string.Empty;
    public string? RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiredAt { get; init; }
    public UserDto User { get; set; } = null!;
    public bool RequiresTwoFactor { get; init; }
}
public record UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public ImmutableList<string> Roles { get; set; } = [];
    public ImmutableList<string> Permissions { get; set; } = [];

}