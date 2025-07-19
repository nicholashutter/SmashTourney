namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class RoundMismatchException : Exception
{
    public RoundMismatchException()
    {

    }

    public RoundMismatchException(string TAG)
    : base($"Game round and player round do not agree. Invalid game state. Exception Originates At TAG {TAG}")
    {
        
    }
}