using AuthorizationServer.Configs;

namespace AuthorizationServer.RateLimit;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RateLimitAttribute : Attribute
{
    public int MaxRequest { get; set; } = 10;
    public int WindowMinutes { get; set; } = 1;
    public RateLimitKeyGen KeyGen { get; set; } = RateLimitKeyGen.IpAddress;
}