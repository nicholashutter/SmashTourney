namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class EmptyGamesCollectionException : Exception
{
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public EmptyGamesCollectionException()
    {
    }

    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public EmptyGamesCollectionException(string TAG)
        : base($"Games Collection Appears Empty. Exception Originates At TAG {TAG}")
    {


    }
}