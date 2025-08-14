using System.Net;
using System.Text.Json;
using AuthorizationServer.Configs;
using AuthorizationServer.RateLimit;
using Microsoft.Extensions.Caching.Distributed;

namespace AuthorizationServer.Middlewares;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly ILogger<RateLimitMiddleware> _logger;

    public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger,
        IDistributedCache cache)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var rateLimitAttr = endpoint?.Metadata.GetMetadata<RateLimitAttribute>();
        if (rateLimitAttr is null)
        {
            await _next(context);
        }

        var key = GenerateKey(context, rateLimitAttr!);
        var currentCount = await GetCurrentCountAsync(key);
        if (currentCount >= rateLimitAttr!.MaxRequest)
        {
            _logger.LogWarning("Rate limit exceeded for key: {Key}", key);
            await HandleExceedRateLimit(context);
            return;
        }

        await IncrementCountAsync(key, rateLimitAttr.WindowMinutes);
        await _next(context);
    }


    private string GenerateKey(HttpContext context, RateLimitAttribute attribute)
    {
        var id = attribute.KeyGen switch
        {
            RateLimitKeyGen.IpAddress => context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            RateLimitKeyGen.User => context.User.Identity?.Name ?? "anonymous",
            RateLimitKeyGen.IpAndUser =>
                $"{context.Connection.RemoteIpAddress}_{context.User.Identity?.Name ?? "anonymous"}",
            _ => context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        };
        return $"rate_limit:{context.Request.Path}:{id}";
    }

    private async Task<int> GetCurrentCountAsync(string key)
    {
        var countStr = await _cache.GetStringAsync(key);
        return int.TryParse(countStr, out var count) ? count : 0;
    }

    private async Task IncrementCountAsync(string key, int windowMinutes)
    {
        var currentCount = await GetCurrentCountAsync(key);
        var newCount = currentCount + 1;
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(windowMinutes)
        };
        await _cache.SetStringAsync(key, newCount.ToString(), options);
    }

    private async Task HandleExceedRateLimit(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";
        var response = new
        {
            error = "Rate limit exceeded",
            message = "Too many requests. Please try again later."
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}