namespace CustomExceptions;

using System;

public class PlayerNotFoundException : Exception
{
    public PlayerNotFoundException()
    {
    }

    public PlayerNotFoundException(string TAG)
        : base($"User Entity Not Found. Exception Originates At TAG {TAG}")
    {


    }
}