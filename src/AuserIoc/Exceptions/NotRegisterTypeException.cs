namespace AuserIoc.Exceptions;

public class NotRegisterTypeException : AuserIocException
{
    public NotRegisterTypeException(Type type) : base($"类型 [{type.FullName}] 未注册")
    {

    }
}
