namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class IdentityUserGetByUserNameException : Exception
{
    public IdentityUserGetByUserNameException(string TAG) : base($"Identity User GetByUserName Failed. Exception Originates At TAG {TAG}")
    {
    }
}