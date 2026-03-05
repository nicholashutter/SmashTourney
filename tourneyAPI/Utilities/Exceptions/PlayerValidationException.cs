namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class PlayerValidationException : Exception
{
    PlayerValidationException()
    {

    }
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public PlayerValidationException(string TAG)
       : base($"Player Entity Invalid. Exception Originates At TAG {TAG}")
    {

    }
}
