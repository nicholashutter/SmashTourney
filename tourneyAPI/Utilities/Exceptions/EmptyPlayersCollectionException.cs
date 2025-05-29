namespace CustomExceptions;

using System;

public class EmptyPlayersCollectionException : Exception
{
    public EmptyPlayersCollectionException()
    {
    }

    public EmptyPlayersCollectionException(string TAG)
        : base($"Players Collection Appears Empty. Exception Originates At TAG {TAG}")
    {


    }
}