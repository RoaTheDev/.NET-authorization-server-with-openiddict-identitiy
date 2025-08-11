namespace Authorization.Domain.interfaces;

public interface IDomainEvent
{
    DateTime OccurredOn { get; set; }
}