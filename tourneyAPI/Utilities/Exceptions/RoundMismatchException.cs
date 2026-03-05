namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class RoundMismatchException : Exception
{
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public RoundMismatchException()
    {

    }

    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public RoundMismatchException(string TAG)
    : base($"Game round and player round do not agree. Invalid game state. Exception Originates At TAG {TAG}")
    {
        
    }
}