using {{Namespace}}.Domain.Common;

namespace {{Namespace}}.Domain.DomainEvents;

public sealed record ProductCreatedDomainEvent(Guid ProductId, string Name) : DomainEvent;
