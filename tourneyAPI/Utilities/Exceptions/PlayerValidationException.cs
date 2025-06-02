namespace CustomExceptions;

public class PlayerValidationException : Exception
{
    PlayerValidationException()
    {

    }
    public PlayerValidationException(string TAG)
       : base($"Player Entity Invalid. Exception Originates At TAG {TAG}")
    {

    }
}
