namespace CustomExceptions;

public class InvalidEntityException<T> : Exception
{
    public T Context;

    public InvalidEntityException(string TAG, T context) : base($"Entity {context} is invalid. Exception Originates At TAG {TAG}")
    {
        Context = context;
    }
}