namespace AuserIoc.Exceptions;

/// <summary>
/// 无法确认接口异常
/// </summary>
public class UnableToDetermineInterfaceException : AuserIocException
{
    /// <summary>
    /// 
    /// </summary>
    internal UnableToDetermineInterfaceException(Type type) : base($"'{type.FullName}' is unable to determine the interface")
    {
    }
}
