namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class IdentityUserCreationException : Exception
{

    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public IdentityUserCreationException(string TAG) : base($"Identity User Creation Failed. Exception Originates At TAG {TAG}")
    {

    }
}