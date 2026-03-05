namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class InvalidObjectStateException : Exception
{
    InvalidObjectStateException()
    {

    }
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public InvalidObjectStateException(string TAG)
       : base($"Invalid Object State. Properties values contradict or otherwise cannot equal their current values. Exception Originates At TAG {TAG}")
    {
    }
}