namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class PlayerNotFoundException : Exception
{
    public PlayerNotFoundException()
    {
    }

    public PlayerNotFoundException(string TAG)
        : base($"Player Entity Not Found. Exception Originates At TAG {TAG}")
    {


    }
}