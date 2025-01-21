namespace AuserIoc.Exceptions;

/// <summary>
/// IocObject配置错误异常
/// </summary>
public class IocObjectConfigurationException : AuserIocException
{
    /// <summary>
    /// IocObject配置错误异常
    /// </summary>
    /// <param name="message"></param>
    //public IocObjectConfigurationException(string message) : base($"IocObject配置错误：{message}")
    internal IocObjectConfigurationException(string message) : base($"IocObject is incorrectly configured: {message}")
    {
    }
}
