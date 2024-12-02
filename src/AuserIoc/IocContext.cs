using AuserIoc.Data;
using System.Collections.Concurrent;
using System.Reflection;

namespace AuserIoc;

internal class IocContext
{
    /// <summary>
    /// 静态锁对象
    /// </summary>
    internal static readonly object STATIC_LOCK = new object();

    /// <summary>
    /// 单例实例对象管理
    /// </summary>
    internal static readonly IocInstanceManage SINGLE_INSTANCE_MANAGE = new IocInstanceManage();

    /// <summary>
    /// 确定泛型参数类型的构造函数字典
    /// </summary>
    internal static readonly ConcurrentDictionary<Type, ConstructorInfo> ACTUAL_TYPE_CONSTRUCTORINFO_MAP = [];
}
