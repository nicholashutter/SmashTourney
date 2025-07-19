namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class EmptyGamesCollectionException : Exception
{
    public EmptyGamesCollectionException()
    {
    }

    public EmptyGamesCollectionException(string TAG)
        : base($"Games Collection Appears Empty. Exception Originates At TAG {TAG}")
    {


    }
}