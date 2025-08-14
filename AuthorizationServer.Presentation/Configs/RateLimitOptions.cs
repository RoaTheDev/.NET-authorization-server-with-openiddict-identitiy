namespace AuthorizationServer.Configs;

public class RateLimitOptions
{
    public int DefaultMaxRequests { get; set; } = 100;
    public int DefaultWindowMinutes { get; set; } = 1;
}