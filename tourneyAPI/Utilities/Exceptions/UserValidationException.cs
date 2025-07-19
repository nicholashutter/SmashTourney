namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class UserValidationException : Exception
{
    UserValidationException()
    {

    }
    public UserValidationException(string TAG)
       : base($"User Entity Invalid. Exception Originates At TAG {TAG}")
    {

    }
}