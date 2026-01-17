namespace Fintech.Exceptions;

public class ConcurrencyException : Exception 
{
    public ConcurrencyException(string msg) : base(msg) { }
}