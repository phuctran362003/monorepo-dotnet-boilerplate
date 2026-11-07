using {{Namespace}}.Domain.Common;
using {{Namespace}}.Domain.Exceptions;

namespace {{Namespace}}.Domain.ValueObjects;

/// <summary>Validated product name value object.</summary>
public sealed class ProductName : ValueObject
{
    public const int MaxLength = 200;

    private ProductName(string value) => Value = value;

    public string Value { get; }

    public static ProductName Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Product name is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
        {
            throw new DomainException($"Product name cannot exceed {MaxLength} characters.");
        }

        return new ProductName(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
