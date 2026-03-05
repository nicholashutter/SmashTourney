namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class GameNotFoundException : Exception
{
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public GameNotFoundException()
    {
    }

    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public GameNotFoundException(string TAG)
        : base($"Game Entity Not Found. Exception Originates At TAG {TAG}")
    {


    }
}