namespace Kinevor.Platform.Domain.Common;

/// <summary>
/// Base class for aggregates. Domain events raised while changing the aggregate are collected
/// here. The host is responsible for persisting and dispatching them (there is no built-in outbox
/// in this standalone module reference implementation).
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Events raised by this aggregate since it was loaded or cleared.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Queues an event for outbox persistence; called only from aggregate behavior.</summary>
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Discards all queued events; called by the unit of work after they are stored.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
