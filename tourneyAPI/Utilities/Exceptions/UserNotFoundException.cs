namespace CustomExceptions;

using System;

public class UserNotFoundException : Exception
{
    public UserNotFoundException()
    {
    }

    public UserNotFoundException(string TAG)
        : base($"User Entity Not Found. Exception Originates At TAG {TAG}")
    {


    }
}