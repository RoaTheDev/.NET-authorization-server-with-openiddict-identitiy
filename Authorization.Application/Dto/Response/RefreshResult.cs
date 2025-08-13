namespace Authorization.Application.Dto.Response;

public record RefreshResult
{
    public Guid UserId { get; init; }
    public bool IsValid { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime ExpiredAt { get; init; }
};