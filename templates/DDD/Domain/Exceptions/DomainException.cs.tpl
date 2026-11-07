namespace {{Namespace}}.Domain.Exceptions;

/// <summary>
/// Raised when a business rule is violated. Infrastructure maps it to an HTTP
/// 400 response with a problem details payload.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
