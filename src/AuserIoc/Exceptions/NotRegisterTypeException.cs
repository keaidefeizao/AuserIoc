namespace AuserIoc.Exceptions;

/// <summary>
/// 未注册类型异常
/// </summary>
public class NotRegisterTypeException : AuserIocException
{
    /// <summary>
    /// 未注册类型异常
    /// </summary>
    /// <param name="type"></param>
    internal NotRegisterTypeException(Type type) : base($"type [{type.FullName}] is not registered")
    {

    }
}
