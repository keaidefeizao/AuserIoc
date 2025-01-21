namespace AuserIoc.Exceptions;

/// <summary>
/// 已注册类型异常
/// </summary>
public class RegisteredTypeException : AuserIocException
{
    /// <summary>
    /// 已注册类型异常
    /// </summary>
    /// <param name="type"></param>
    internal RegisteredTypeException(Type type) : base($"Type [{type.FullName}] has been registered")
    {
    }
}
