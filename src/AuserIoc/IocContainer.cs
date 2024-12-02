using AuserIoc.Data;
using AuserIoc.Exceptions;

namespace AuserIoc;

/// <summary>
/// ioc 容器
/// </summary>
public class IocContainer : IIocContainer
{
    private readonly object _lock = new object();

    [ThreadStatic]
    private static HashSet<Type>? _resolvingTypes; // 用于追踪当前解析的类型

    private readonly IocObjectManage _iocObjectManage;
    private readonly IReadOnlyDictionary<string, IocObject> _iocObjectNameMap;
    private readonly IocInstanceManage _scopeInstanceManage;

    internal IocContainer(IReadOnlyDictionary<Type, IocObject> iocObjectMap)
    {
        _iocObjectManage = new IocObjectManage(iocObjectMap);

        _iocObjectNameMap = iocObjectMap.Values.Where(t => !string.IsNullOrWhiteSpace(t.Name)).ToDictionary(obj => obj.Name!, obj => obj)!;

        _scopeInstanceManage = new IocInstanceManage();
    }

    internal void Initialize()
    {
        // 默认注入当前容器
        var iocContainerType = typeof(IocContainer);

        if (_iocObjectManage.Map.TryGetValue(iocContainerType, out var iocObject))
        {
            iocObject
                .AddFactoryMethod(() => this)
                .InstanceByContainerScope();
        }

        _ = Resolve<IIocContainer>();
    }

    public IIocContainer BeginContainerScope()
    {
        var container = new IocContainer(_iocObjectManage.Map);

        container.Initialize();

        return container;
    }

    public T Resolve<T>()
    {
        var type = typeof(T);
        return (T)Resolve(type);
    }

    public T Resolve<T>(string name)
    {
        if (_iocObjectNameMap.TryGetValue(name, out IocObject? iocObject) && iocObject is not null)
        {
            return (T)Resolve(iocObject.Type, iocObject);
        }

        throw new IocResolveException($"找不到名称为 [{name}] 注册对象");
    }

    public object Resolve(Type type)
    {
        // 初始化线程本地的解析上下文
        _resolvingTypes ??= new HashSet<Type>();

        // 检测循环依赖
        if (_resolvingTypes.Contains(type))
        {
            // 抛出循环依赖异常
            throw new CircularDependencyException(type);
        }

        try
        {
            _resolvingTypes.Add(type); // 开始解析类型
            IocObject iocObject = _iocObjectManage[type];
            return Resolve(type, iocObject);
        }
        finally
        {
            _resolvingTypes.Remove(type); // 解析完成后移除类型
        }
    }

    private object Resolve(IocObject iocObject)
    {
        return Resolve(iocObject.Type, iocObject);
    }

    private object Resolve(Type type, IocObject iocObject)
    {
        if (iocObject.Instance is not null && iocObject.InstanceResolveType != InstanceResolveType.Singleton)
        {
            return iocObject.Instance;
        }

        switch (iocObject.InstanceResolveType)
        {
            case InstanceResolveType.Singleton:
                return ResolveInstanceBySingleton(type, iocObject);

            case InstanceResolveType.PerDependency:
                return ResolveInstanceByPerDependency(type, iocObject);

            case InstanceResolveType.ContainerScope:
                return ResolveInstanceByContainerScope(type, iocObject);
            default:
                throw new NotImplementedException();
        }
    }

    private object ResolveInstanceBySingleton(Type type, IocObject iocObject)
    {
        if (IocContext.SINGLE_INSTANCE_MANAGE.TryGetValue(iocObject, out var instance))
        {
            return instance!;
        }

        lock (IocContext.STATIC_LOCK)  // 只在需要创建实例时加锁
        {
            if (!IocContext.SINGLE_INSTANCE_MANAGE.ContainsKey(iocObject))
            {
                var newInstance = iocObject.Instance;

                if (iocObject.Instance is null)
                {
                    newInstance = GetInstance(type, iocObject);
                }

                IocContext.SINGLE_INSTANCE_MANAGE.Add(iocObject, newInstance!);
                return newInstance!;
            }

            return IocContext.SINGLE_INSTANCE_MANAGE[iocObject];
        }
    }

    private object ResolveInstanceByContainerScope(Type type, IocObject iocObject)
    {
        if (_scopeInstanceManage.TryGetValue(iocObject, out var instance))
        {
            return instance!;
        }

        lock (_lock)  // 只在需要创建实例时加锁
        {
            if (!_scopeInstanceManage.ContainsKey(iocObject))
            {
                var newInstance = GetInstance(type, iocObject);
                _scopeInstanceManage.Add(iocObject, newInstance);
                return newInstance;
            }

            return _scopeInstanceManage[iocObject];
        }
    }

    private object ResolveInstanceByPerDependency(Type type, IocObject iocObject)
    {
        return GetInstance(type, iocObject);
    }

    private object GetInstance(Type type, IocObject iocObject)
    {
        var parametersResolve = () =>
        {
            var parameterInfos = iocObject.ParameterInfos;

            var parameters = new object[parameterInfos.Length];

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                parameters[i] = Resolve(parameterInfos[i].ParameterType);
            }

            return parameters;
        };

        if (iocObject.FactoryMethod is not null)
        {
            return iocObject.FactoryMethod.DynamicInvoke(parametersResolve())!;
        }
        else
        {
            var constructor = iocObject.GetConstructorInfo(type);

            return constructor.Invoke(parametersResolve());
        }
    }
}