namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class UserNotFoundException : Exception
{
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public UserNotFoundException()
    {
    }

    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public UserNotFoundException(string TAG)
        : base($"User Entity Not Found. Exception Originates At TAG {TAG}")
    {


    }
}