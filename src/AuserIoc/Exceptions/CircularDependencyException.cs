namespace AuserIoc.Exceptions;

public class CircularDependencyException : IocResolveException
{
    public CircularDependencyException(Type exceptionType) : base($"类型 [{exceptionType.FullName}] 产生了循环依赖")
    {
    }
}
