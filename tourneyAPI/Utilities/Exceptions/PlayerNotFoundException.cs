namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class PlayerNotFoundException : Exception
{
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public PlayerNotFoundException()
    {
    }

    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public PlayerNotFoundException(string TAG)
        : base($"Player Entity Not Found. Exception Originates At TAG {TAG}")
    {


    }
}