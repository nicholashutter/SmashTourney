namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class BracketGenrationException : Exception
{

    public BracketGenrationException(string TAG) : base($"Bracket could not be generated. Exception Originates At TAG {TAG}")
    {

    }
}