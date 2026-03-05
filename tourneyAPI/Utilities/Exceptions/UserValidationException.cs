namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class UserValidationException : Exception
{
    UserValidationException()
    {

    }
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public UserValidationException(string TAG)
       : base($"User Entity Invalid. Exception Originates At TAG {TAG}")
    {

    }
}