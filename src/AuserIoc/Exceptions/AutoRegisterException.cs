namespace AuserIoc.Exceptions;

/// <summary>
/// 自动注册异常
/// </summary>
public class AutoRegisterException : AuserIocException
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message"></param>
    internal AutoRegisterException(Type type, string message) : base($"Automatic registration '{type.FullName}' exception: {message}")
    {
    }
}
