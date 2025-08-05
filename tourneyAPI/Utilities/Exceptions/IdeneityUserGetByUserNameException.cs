namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class IdentityUserGetAllException : Exception
{
    public IdentityUserGetAllException(string TAG) : base($"Identity User GetAll Failed. Exception Originates At TAG {TAG}")
    {
    }
}