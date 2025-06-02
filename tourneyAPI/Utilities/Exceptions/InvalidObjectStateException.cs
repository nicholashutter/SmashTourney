namespace CustomExceptions;

public class InvalidObjectStateException : Exception
{
    InvalidObjectStateException()
    {

    }
    public InvalidObjectStateException(string TAG)
       : base($"Invalid Object State. Properties values contradict or otherwise cannot equal their current values. Exception Originates At TAG {TAG}")
    {
    }
}