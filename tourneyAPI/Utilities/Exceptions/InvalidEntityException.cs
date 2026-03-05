namespace CustomExceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents a domain-specific failure used to communicate API business-rule errors.
public class InvalidEntityException<T> : Exception
{
    public T Context;

    public InvalidEntityException(string TAG, T context) : base($"Entity {context} is invalid. Exception Originates At TAG {TAG}")
    {
        Context = context;
    }
}