namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class EmptyUsersCollectionException : Exception
{
    public EmptyUsersCollectionException()
    {
    }

    public EmptyUsersCollectionException(string TAG)
        : base($"Users Collection Appears Empty. Exception Originates At TAG {TAG}")
    {


    }
}