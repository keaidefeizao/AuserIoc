namespace AuserIoc;

/// <summary>
/// ioc 容器接口
/// </summary>
public interface IIocContainer : IDisposable
{
    /// <summary>
    /// 开始一个新的容器
    /// </summary>
    /// <returns></returns>
    IIocContainer BeginContainerScope();

    /// <summary>
    /// 解析获取对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Resolve<T>();

    /// <summary>
    /// 通过名称解析狐裘指定的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    T Resolve<T>(string name);

    /// <summary>
    /// 通过类型解析获取对象
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    object Resolve(Type type);
}
