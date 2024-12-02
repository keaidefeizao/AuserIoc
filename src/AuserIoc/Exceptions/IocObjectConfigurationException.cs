namespace AuserIoc.Exceptions;

public class IocObjectConfigurationException : AuserIocException
{
    public IocObjectConfigurationException(string message) : base($"IocObject配置错误：{message}")
    {
    }
}
