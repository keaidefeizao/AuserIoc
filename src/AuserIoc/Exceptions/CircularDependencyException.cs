namespace AuserIoc.Exceptions;

/// <summary>
/// 循环依赖异常
/// </summary>
public class CircularDependencyException : IocResolveException
{
    /// <summary>
    /// 循环依赖异常的实例化
    /// </summary>
    /// <param name="exceptionType">循环依赖异常的类型</param>
    //public CircularDependencyException(Type exceptionType) : base($"类型 [{exceptionType.FullName}] 产生了循环依赖")
    internal CircularDependencyException(Type exceptionType) : base($"The type [{exceptionType.FullName}] creates a loop dependency")
    {
    }
}
