namespace MonorepoDotnet.Core.Generation;

/// <summary>Raised when a generation step fails with a user-actionable message.</summary>
public sealed class GenerationException : Exception
{
    public GenerationException(string message) : base(message)
    {
    }

    public GenerationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
