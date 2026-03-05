namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class InvalidFunctionResponseException : Exception
{
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public InvalidFunctionResponseException()
    {
    }

    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public InvalidFunctionResponseException(string TAG)
        : base($"Response from function not allowed or null. Exception Originates At TAG {TAG}")
    {


    }
}