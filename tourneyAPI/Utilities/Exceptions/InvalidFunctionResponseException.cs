namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class InvalidFunctionResponseException : Exception
{
    public InvalidFunctionResponseException()
    {
    }

    public InvalidFunctionResponseException(string TAG)
        : base($"Response from function not allowed or null. Exception Originates At TAG {TAG}")
    {


    }
}