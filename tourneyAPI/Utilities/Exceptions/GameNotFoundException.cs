namespace CustomExceptions;

using System;

public class GameNotFoundException : Exception
{
    public GameNotFoundException()
    {
    }

    public GameNotFoundException(string TAG)
        : base($"Game Entity Not Found. Exception Originates At TAG {TAG}")
    {


    }
}