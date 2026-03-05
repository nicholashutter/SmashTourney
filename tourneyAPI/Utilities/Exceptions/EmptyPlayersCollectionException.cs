namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class EmptyPlayersCollectionException : Exception
{
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public EmptyPlayersCollectionException()
    {
    }

    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public EmptyPlayersCollectionException(string TAG)
        : base($"Players Collection Appears Empty. Exception Originates At TAG {TAG}")
    {


    }
}