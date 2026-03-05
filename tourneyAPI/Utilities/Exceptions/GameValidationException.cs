namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class GameValidationException : Exception
{
    GameValidationException()
    {

    }
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public GameValidationException(string TAG)
       : base($"Game Entity Invalid. Exception Originates At TAG {TAG}")
    {

    }
}