namespace AuserIoc.Exceptions;

public abstract class AuserIocException : Exception
{
    public AuserIocException(string message) : base(message)
    {
    }

    public AuserIocException() : base()
    { 
    }
}
