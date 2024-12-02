namespace AuserIoc.Exceptions;

public class RegisteredException : AuserIocException
{
    public RegisteredException(Type type) : base($"类型 {type.FullName} 已经注册")
    {
    }
}
