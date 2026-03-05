namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class IdentityUserGetByIdException : Exception
{
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public IdentityUserGetByIdException(string TAG) : base($"Identity User GetById Failed. Exception Originates At TAG {TAG}")
    {
    }
}