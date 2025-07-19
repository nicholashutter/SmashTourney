namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class InvalidArgumentException : Exception
{
    public InvalidArgumentException()
    {
    }

    public InvalidArgumentException(string TAG)
        : base($"Argument to function not allowed or null. Exception Originates At TAG {TAG}")
    {


    }
}