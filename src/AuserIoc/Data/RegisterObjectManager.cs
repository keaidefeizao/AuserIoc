using AuserIoc.Exceptions;
using System.Collections.Concurrent;

namespace AuserIoc.Data;

internal class RegisterObjectManager(IReadOnlyDictionary<Type, RegisterObject> IocObjectMap)
{
    private readonly ConcurrentDictionary<Type, RegisterObject> _cache = new(IocObjectMap);
    private readonly ConcurrentDictionary<Type, Type> _genericTypeDefinitionCache = new();
    private readonly ConcurrentDictionary<Type, RegisterObject> _genericTypeDefinitionToRegisterCache = new();

    internal RegisterObject this[Type type]
    {
        get
        {
            return Get(type);
        }
    }

    internal IReadOnlyDictionary<Type, RegisterObject> Map => IocObjectMap;

    private RegisterObject Get(Type type)
    {
        // 首先尝试从缓存中获取
        if (_cache.TryGetValue(type, out var registerObject))
        {
            return registerObject;
        }

        // 尝试从原始字典中获取
        if (IocObjectMap.TryGetValue(type, out registerObject))
        {
            _cache.TryAdd(type, registerObject);
            return registerObject;
        }

        // 尝试泛型类型匹配
        if (type.IsGenericType)
        {
            var genericTypeDefinition = _genericTypeDefinitionCache.GetOrAdd(type, t => t.GetGenericTypeDefinition());

            // 检查泛型类型定义是否已缓存
            if (_cache.TryGetValue(genericTypeDefinition, out registerObject))
            {
                _cache.TryAdd(type, registerObject);
                return registerObject;
            }

            // 尝试从泛型类型定义索引缓存中获取
            if (_genericTypeDefinitionToRegisterCache.TryGetValue(genericTypeDefinition, out registerObject))
            {
                _cache.TryAdd(type, registerObject);
                return registerObject;
            }

            // 查找实现了泛型类型定义的注册对象，优先返回无名称的注册
            foreach (var item in IocObjectMap)
            {
                if (item.Value.ParentType != null)
                {
                    var parentGenericType = item.Value.ParentType.IsGenericType
                        ? item.Value.ParentType.GetGenericTypeDefinition()
                        : item.Value.ParentType;

                    if (parentGenericType == genericTypeDefinition && string.IsNullOrEmpty(item.Value.Name))
                    {
                        _cache.TryAdd(type, item.Value);
                        _genericTypeDefinitionToRegisterCache.TryAdd(genericTypeDefinition, item.Value);
                        return item.Value;
                    }
                }
            }

            // 如果没有找到无名称的注册，查找所有匹配的
            foreach (var item in IocObjectMap)
            {
                if (item.Value.ParentType != null)
                {
                    var parentGenericType = item.Value.ParentType.IsGenericType
                        ? item.Value.ParentType.GetGenericTypeDefinition()
                        : item.Value.ParentType;

                    if (parentGenericType == genericTypeDefinition)
                    {
                        _cache.TryAdd(type, item.Value);
                        _genericTypeDefinitionToRegisterCache.TryAdd(genericTypeDefinition, item.Value);
                        return item.Value;
                    }
                }
            }
        }
        else
        {
            // 非泛型类型，查找 ParentType 匹配的，优先返回无名称的注册
            foreach (var item in IocObjectMap)
            {
                if (item.Value.ParentType == type && string.IsNullOrEmpty(item.Value.Name))
                {
                    _cache.TryAdd(type, item.Value);
                    return item.Value;
                }
            }

            // 如果没有找到无名称的注册，查找所有匹配的
            foreach (var item in IocObjectMap)
            {
                if (item.Value.ParentType == type)
                {
                    _cache.TryAdd(type, item.Value);
                    return item.Value;
                }
            }
        }

        throw new NotRegisterTypeException(type);
    }
}
