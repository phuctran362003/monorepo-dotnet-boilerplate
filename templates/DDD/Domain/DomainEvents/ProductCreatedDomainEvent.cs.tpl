using {{DomainNamespace}}.Common;

namespace {{DomainNamespace}}.DomainEvents;

public sealed record ProductCreatedDomainEvent(Guid ProductId, string Name) : DomainEvent;
