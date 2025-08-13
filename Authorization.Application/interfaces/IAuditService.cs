namespace Authorization.Application.interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string resource, bool success,
        string? details = null, string? ipAddress = null, Guid? userId = null);

    Task LogEntityChangeAsync<T>(T entity, string action, T? oldEntity = null) where T : class;
    
}