namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class IdentityUserGetByIdException : Exception
{
    public IdentityUserGetByIdException(string TAG) : base($"Identity User GetById Failed. Exception Originates At TAG {TAG}")
    {
    }
}