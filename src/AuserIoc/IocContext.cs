using AuserIoc.Common;
using AuserIoc.Data;
using System.Collections.Concurrent;

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
    /// 确定泛型参数类型的解析信息数字典
    /// </summary>
    internal static readonly ConcurrentDictionary<Type, TypeResolveInfo> ACTUAL_TYPE_TYPERESOLVEINFO_MAP = [];
}
