namespace {{Namespace}}.Exceptions;

/// <summary>Raised when a business rule is violated.</summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
