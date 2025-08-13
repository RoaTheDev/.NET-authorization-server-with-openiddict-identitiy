namespace Authorization.Application.interfaces;

public interface IDateTimeService
{
    public DateTime UtcNow { get; }
    public DateTime Now { get; }
    public DateOnly Today { get; }
}