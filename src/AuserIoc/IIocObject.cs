namespace AuserIoc;

/// <summary>
/// ioc 对象
/// </summary>
public interface IIocObject
{
    /// <summary>
    /// 实现类型
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// 父类类型
    /// </summary>
    Type? ParentType { get; }

    /// <summary>
    /// 名称
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// ioc对象实例类型
    /// </summary>
    IIocObjectInstanceEnum InstanceType { get; }

    /// <summary>
    /// 控制反转类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IIocObject As<T>();

    /// <summary>
    /// 控制反转类型
    /// </summary>
    /// <param name="parentTypeFullName"></param>
    /// <returns></returns>
    IIocObject As(string parentTypeFullName);

    /// <summary>
    /// 设置名称
    /// 如果对象注册了多个实例的实现则通过名称取到指定的实现
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IIocObject SetName(string name);

    /// <summary>
    /// 单例注册
    /// </summary>
    /// <returns></returns>
    IIocObject SingleInstance();

    /// <summary>
    /// 每个都新实例注册
    /// </summary>
    /// <returns></returns>
    IIocObject InstancePerDependency();

    /// <summary>
    /// 在范围内保持一个的实例注册
    /// </summary>
    /// <returns></returns>
    IIocObject InstancePerLifetimeScope();
}
