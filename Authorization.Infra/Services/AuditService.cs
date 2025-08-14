using Authorization.Application.interfaces;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Data;

namespace Authorization.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AuthDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AuditService(AuthDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task LogAsync(string action, string resource, bool success, string? details = null,
        string? ipAddress = null,
        Guid? userId = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId ?? _currentUserService.UserId,
            Action = action,
            Resource = resource,
            Success = success,
            IpAddress = ipAddress,
            TimeStamp = DateTime.UtcNow,
            NewValue = details
        };

        if (!success && !string.IsNullOrEmpty(details))
        {
            auditLog.ErrorMessage = details;
        }

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task LogEntityChangeAsync<T>(T entity, string action, T? oldEntity = null) where T : class
    {
        var entityName = typeof(T).Name;
        var auditLog = new AuditLog
        {
            UserId = _currentUserService.UserId,
            Action = action,
            Resource = entityName,
            Success = true,
            TimeStamp = DateTime.UtcNow
        };

        if (oldEntity != null)
        {
            auditLog.OldValue = System.Text.Json.JsonSerializer.Serialize(oldEntity);
        }

        auditLog.NewValue = System.Text.Json.JsonSerializer.Serialize(entity);

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}