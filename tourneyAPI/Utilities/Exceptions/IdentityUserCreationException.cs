namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class IdentityUserCreationException : Exception
{

    public IdentityUserCreationException(string TAG) : base($"Identity User Creation Failed. Exception Originates At TAG {TAG}")
    {

    }
}