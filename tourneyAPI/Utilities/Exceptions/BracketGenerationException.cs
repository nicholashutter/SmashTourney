namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class BracketGenrationException : Exception
{

    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public BracketGenrationException(string TAG) : base($"Bracket could not be generated. Exception Originates At TAG {TAG}")
    {

    }
}