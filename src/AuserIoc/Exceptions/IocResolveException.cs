namespace AuserIoc.Exceptions;

public class IocResolveException : AuserIocException
{
    public IocResolveException(string message) : base($"解析错误：{message}")
    {
    }
}
