namespace CustomExceptions;

public class GameValidationException : Exception
{
    GameValidationException()
    {

    }
    public GameValidationException(string TAG)
       : base($"Game Entity Invalid. Exception Originates At TAG {TAG}")
    {

    }
}