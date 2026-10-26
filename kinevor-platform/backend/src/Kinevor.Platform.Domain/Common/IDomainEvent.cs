namespace Kinevor.Platform.Domain.Common;

/// <summary>
/// Marker for events raised by aggregates. The host is responsible for persisting and dispatching
/// these events; there is no built-in outbox in this standalone module reference implementation.
/// </summary>
public interface IDomainEvent;
