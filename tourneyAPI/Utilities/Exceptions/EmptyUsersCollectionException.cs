namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class EmptyUsersCollectionException : Exception
{
    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public EmptyUsersCollectionException()
    {
    }

    // Initializes this exception with context needed for troubleshooting and client-safe messaging.
    public EmptyUsersCollectionException(string TAG)
        : base($"Users Collection Appears Empty. Exception Originates At TAG {TAG}")
    {


    }
}