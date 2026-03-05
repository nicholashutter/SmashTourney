namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class IdentityUserGetByUserNameException : Exception
{
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public IdentityUserGetByUserNameException(string TAG) : base($"Identity User GetByUserName Failed. Exception Originates At TAG {TAG}")
    {
    }
}