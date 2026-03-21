using AuserIoc.Common;
using AuserIoc.Data;
using AuserIoc.Exceptions;
using System.Collections.Concurrent;
using System.Reflection;

namespace AuserIoc;

/// <summary>
/// ioc 容器
/// </summary>
public sealed class IocContainer : IIocContainer
{
#if NET9_0
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

    [ThreadStatic]
    private static HashSet<Type>? _resolvingTypes; // 用于追踪当前解析的类型

    private readonly RegisterObjectManager _registerObjectManager;
    private readonly Dictionary<string, RegisterObject> _registerObjectNameMap;
    private readonly ConcurrentDictionary<RegisterObject, object> _scopeInstanceManager;

    internal IocContainer(IReadOnlyDictionary<Type, RegisterObject> registerObjectMap)
    {
        _registerObjectManager = new RegisterObjectManager(registerObjectMap);

        // 优化：使用 for 循环替代 LINQ，减少分配
        var namedRegisters = new List<KeyValuePair<Type, RegisterObject>>(registerObjectMap.Count);
        foreach (var kvp in registerObjectMap)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Value.Name))
            {
                namedRegisters.Add(kvp);
            }
        }

        _registerObjectNameMap = new Dictionary<string, RegisterObject>(namedRegisters.Count);
        foreach (var kvp in namedRegisters)
        {
            _registerObjectNameMap[kvp.Value.Name!] = kvp.Value;
        }

        _scopeInstanceManager = [];
    }

    internal void Initialize()
    {
        // 默认注入当前容器
        var iocContainerType = typeof(IocContainer);

        if (_registerObjectManager.Map.TryGetValue(iocContainerType, out var iocObject))
        {
            iocObject
                .AddFactoryMethod(() => this)
                .InstanceByContainerScope();
        }

        _ = Resolve<IIocContainer>();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _scopeInstanceManager.Clear();
    }

    /// <inheritdoc/>
    public IIocContainer BeginContainerScope()
    {
        var container = new IocContainer(_registerObjectManager.Map);

        container.Initialize();

        return container;
    }

    /// <inheritdoc/>
    public T Resolve<T>()
    {
        var type = typeof(T);
        return (T)Resolve(type);
    }

    /// <inheritdoc/>
    public T Resolve<T>(string name)
    {
        if (_registerObjectNameMap.TryGetValue(name, out RegisterObject? iocObject) && iocObject is not null)
        {
            return (T)Resolve(typeof(T), iocObject);
        }

        throw new IocResolveException($"Registration object with name [{name}] not found");
    }

    /// <inheritdoc/>
    public object Resolve(Type type)
    {
        // 初始化线程本地的解析上下文
        _resolvingTypes ??= [];

        // 检测循环依赖
        if (_resolvingTypes.Contains(type))
        {
            // 抛出循环依赖异常
            throw new CircularDependencyException(type);
        }

        try
        {
            _resolvingTypes.Add(type); // 开始解析类型
            RegisterObject registerObject = _registerObjectManager[type];
            return Resolve(type, registerObject);
        }
        finally
        {
            _resolvingTypes.Remove(type); // 解析完成后移除类型
        }
    }

    private object Resolve(RegisterObject iocObject)
    {
        return Resolve(iocObject.Type, iocObject);
    }

    private object Resolve(Type type, RegisterObject registerObject)
    {
        if (registerObject.Instance is not null && registerObject.InstanceResolveType != InstanceResolveType.Singleton)
        {
            return registerObject.Instance;
        }

        return registerObject.InstanceResolveType switch
        {
            InstanceResolveType.Singleton => ResolveInstanceBySingleton(type, registerObject),
            InstanceResolveType.PerDependency => ResolveInstanceByPerDependency(type, registerObject),
            InstanceResolveType.ContainerScope => ResolveInstanceByContainerScope(type, registerObject),
            _ => throw new NotImplementedException(),
        };
    }

    private object ResolveInstanceBySingleton(Type type, RegisterObject registerObject)
    {
        return IocContext.SINGLE_INSTANCE_MANAGE.GetOrAdd(registerObject, key =>
        {
            return registerObject.Instance ?? GetInstance(type, key);
        });
    }

    private object ResolveInstanceByContainerScope(Type type, RegisterObject registerObject)
    {
        // 先尝试无锁获取
        if (_scopeInstanceManager.TryGetValue(registerObject, out var instance))
        {
            return instance!;
        }

        // 需要创建新实例时加锁
        lock (_lock)
        {
            if (_scopeInstanceManager.TryGetValue(registerObject, out instance))
            {
                return instance!;
            }

            var newInstance = GetInstance(type, registerObject);
            _scopeInstanceManager.AddOrUpdate(registerObject, newInstance, (_, _) => newInstance);
            return newInstance;
        }
    }

    private object ResolveInstanceByPerDependency(Type type, RegisterObject registerObject)
    {
        return GetInstance(type, registerObject);
    }

    private object GetInstance(Type type, RegisterObject registerObject)
    {
        if (registerObject.FactoryMethod is not null)
        {
            var parameters = registerObject.FactoryMethodParameterInfos;
            if (parameters.Length == 0)
            {
                // 无参数工厂方法：尝试直接调用委托，避免 DynamicInvoke 开销
                return registerObject.FactoryMethod switch
                {
                    Func<object> simpleFactory => simpleFactory(),
                    Func<IIocContainer, object> factoryWithContainer => factoryWithContainer(this),
                    _ => registerObject.FactoryMethod.DynamicInvoke(null)!
                };
            }
            else
            {
                // 有参数工厂方法：解析参数后调用
                var resolvedParams = ResolveParameters(parameters);

                // 尝试使用委托直接调用，避免 DynamicInvoke 开销
                return registerObject.FactoryMethod switch
                {
                    Func<object[], object> arrayFactory => arrayFactory(resolvedParams),
                    _ => registerObject.FactoryMethod.DynamicInvoke(resolvedParams)!
                };
            }
        }
        else
        {
            var typeResolveInfo = registerObject.GetTypeResolveInfo(type);
            var parameters = typeResolveInfo.ParameterInfos;
            // 使用预编译的构造函数调用器
            return typeResolveInfo.Invoke(parameters.Length == 0 ? null : ResolveParameters(parameters));
        }
    }

    private object[] ResolveParameters(ParameterInfo[] parameterInfos)
    {
        var parameters = new object[parameterInfos.Length];

        for (int i = 0; i < parameterInfos.Length; i++)
        {
            parameters[i] = Resolve(parameterInfos[i].ParameterType);
        }

        return parameters;
    }
}