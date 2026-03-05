namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class InvalidArgumentException : Exception
{
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public InvalidArgumentException()
    {
    }

    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public InvalidArgumentException(string TAG)
        : base($"Argument to function not allowed or null. Exception Originates At TAG {TAG}")
    {


    }
}