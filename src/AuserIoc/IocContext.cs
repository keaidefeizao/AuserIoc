using AuserIoc.Common;
using AuserIoc.Data;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AuserIoc;

internal class IocContext
{
    /// <summary>
    /// 静态锁对象
    /// </summary>
    internal static readonly object STATIC_LOCK = new();

    /// <summary>
    /// 单例实例对象管理
    /// </summary>
    internal static readonly IocInstanceManager SINGLE_INSTANCE_MANAGE = new IocInstanceManager();

    /// <summary>
    /// 确定泛型参数类型的解析信息数字典
    /// 缓存键为 (解析类型，实现类型) 的组合
    /// 使用 ConditionalWeakTable 允许 GC 在类型不再被引用时自动清理缓存
    /// </summary>
    internal static readonly ConditionalWeakTable<Type, TypeResolveInfoCache> ACTUAL_TYPE_RESOLVE_INFO_MAP = new();

    /// <summary>
    /// 用于存储每个解析类型对应的实现类型缓存
    /// </summary>
    internal sealed class TypeResolveInfoCache : ConcurrentDictionary<Type, TypeResolveInfo>
    {
    }

    /// <summary>
    /// 用于 ACTUAL_TYPE_RESOLVE_INFO_MAP 缓存的键类型
    /// </summary>
    internal sealed class TypeResolveInfoCacheKey : IEquatable<TypeResolveInfoCacheKey>
    {
        public Type ResolveType { get; }
        public Type ImplType { get; }

        public TypeResolveInfoCacheKey(Type resolveType, Type implType)
        {
            ResolveType = resolveType;
            ImplType = implType;
        }

        public bool Equals(TypeResolveInfoCacheKey? other)
        {
            if (other is null) return false;
            return ReferenceEquals(this, other) ||
                   (ReferenceEquals(ResolveType, other.ResolveType) &&
                    ReferenceEquals(ImplType, other.ImplType));
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TypeResolveInfoCacheKey);
        }

        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(ResolveType) ^
                   System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(ImplType);
        }
    }
}