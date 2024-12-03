namespace AuserIoc.Exceptions;

/// <summary>
/// ioc解析异常
/// </summary>
public class IocResolveException : AuserIocException
{
    /// <summary>
    /// 带自定义消息的ioc解析异常实例
    /// </summary>
    /// <param name="message"></param>
    public IocResolveException(string message) : base($"Resolving error：{message}")
    {
    }
}
