namespace AuserIoc.Common;

/// <summary>
/// ioc对象实例类型枚举
/// </summary>
public enum InstanceResolveType
{
    /// <summary>
    /// 单例
    /// </summary>
    Singleton,

    /// <summary>
    /// 每次解析都是一个新实例
    /// </summary>
    PerDependency,

    /// <summary>
    /// 一个容器范围内保持一个实例
    /// </summary>
    ContainerScope,

    /// <summary>
    /// 
    /// </summary>
    None,
}
