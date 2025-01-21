namespace AuserIoc.Exceptions;

/// <summary>
/// AuserIoc异常
/// </summary>
public abstract class AuserIocException : Exception
{
    /// <summary>
    /// 带自定义消息的实例化
    /// </summary>
    /// <param name="message"></param>
    protected AuserIocException(string message) : base(message)
    {
    }

    /// <summary>
    /// 实例化
    /// </summary>
    protected AuserIocException() : base()
    { 
    }
}
