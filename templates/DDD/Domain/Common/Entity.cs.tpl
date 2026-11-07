namespace {{Namespace}}.Domain.Common;

/// <summary>
/// Base type for domain entities. Entities are identified by their id, not by
/// their attribute values.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
