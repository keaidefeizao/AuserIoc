using AuserIoc.Common;
using AuserIoc.Data;
using AuserIoc.Exceptions;
using System.Reflection;

namespace AuserIoc;

/// <summary>
/// ioc 容器
/// </summary>
public class IocContainer : IIocContainer
{
    private readonly object _lock = new object();

    [ThreadStatic]
    private static HashSet<Type>? _resolvingTypes; // 用于追踪当前解析的类型

    private readonly RegisterObjectManage _registerObjectManage;
    private readonly IReadOnlyDictionary<string, RegisterObject> _registerObjectNameMap;
    //private readonly IocInstanceManage _scopeInstanceManage;
    private readonly Dictionary<RegisterObject, WeakReference<object>> _scopeInstanceManage;

    internal IocContainer(IReadOnlyDictionary<Type, RegisterObject> registerObjectMap)
    {
        _registerObjectManage = new RegisterObjectManage(registerObjectMap);

        _registerObjectNameMap = registerObjectMap.Values.Where(t => !string.IsNullOrWhiteSpace(t.Name)).ToDictionary(obj => obj.Name!, obj => obj)!;

        _scopeInstanceManage = [];
    }

    internal void Initialize()
    {
        // 默认注入当前容器
        var iocContainerType = typeof(IocContainer);

        if (_registerObjectManage.Map.TryGetValue(iocContainerType, out var iocObject))
        {
            iocObject
                .AddFactoryMethod(() => this)
                .InstanceByContainerScope();
        }

        _ = Resolve<IIocContainer>();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        //GC.SuppressFinalize(this);
        //throw new NotImplementedException();
        lock (_lock)
        {
            _scopeInstanceManage.Clear();
            GC.Collect();
        }
    }

    /// <inheritdoc/>
    public IIocContainer BeginContainerScope()
    {
        var container = new IocContainer(_registerObjectManage.Map);

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
            return (T)Resolve(iocObject.Type, iocObject);
        }

        throw new IocResolveException($"Registration object with name [{name}] not found");
    }

    /// <inheritdoc/>
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
            RegisterObject registerObject = _registerObjectManage[type];
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

        switch (registerObject.InstanceResolveType)
        {
            case InstanceResolveType.Singleton:
                return ResolveInstanceBySingleton(type, registerObject);

            case InstanceResolveType.PerDependency:
                return ResolveInstanceByPerDependency(type, registerObject);

            case InstanceResolveType.ContainerScope:
                return ResolveInstanceByContainerScope(type, registerObject);
            default:
                throw new NotImplementedException();
        }
    }

    private object TakeInstance(IocInstanceManage iocInstanceManage, object lockObject, Type type, RegisterObject registerObject)
    {
        if (iocInstanceManage.TryGetValue(registerObject, out var instance))
        {
            return instance;
        }

        lock (lockObject)
        {
            if (!iocInstanceManage.ContainsKey(registerObject))
            {
                var newInstance = registerObject.Instance;

                if (newInstance is null)
                {
                    newInstance = GetInstance(type, registerObject);
                }

                iocInstanceManage.Add(registerObject, newInstance!);

                return newInstance!;
            }

            return iocInstanceManage[registerObject];
        }
    }

    private object ResolveInstanceBySingleton(Type type, RegisterObject registerObject)
    {
        return TakeInstance(IocContext.SINGLE_INSTANCE_MANAGE, IocContext.STATIC_LOCK, type, registerObject);
    }

    private object ResolveInstanceByContainerScope(Type type, RegisterObject iocObject)
    {
        if (_scopeInstanceManage.TryGetValue(iocObject, out var weakReference) && weakReference.TryGetTarget(out var instance))
        {
            return instance!;
        }

        lock (_lock)  // 只在需要创建实例时加锁
        {
            if (!_scopeInstanceManage.ContainsKey(iocObject))
            {
                var newInstance = GetInstance(type, iocObject);
                _scopeInstanceManage.Add(iocObject, new WeakReference<object>(newInstance));
                return newInstance;
            }

            if (_scopeInstanceManage[iocObject].TryGetTarget(out instance))
            {
                return instance;
            }
            else
            {
                var newInstance = GetInstance(type, iocObject);
                _scopeInstanceManage[iocObject].SetTarget(newInstance);
                return newInstance;
            }
        }
        //return TakeInstance(_scopeInstanceManage, _lock, type, iocObject);
    }

    private object ResolveInstanceByPerDependency(Type type, RegisterObject registerObject)
    {
        return GetInstance(type, registerObject);
    }

    private object GetInstance(Type type, RegisterObject registerObject)
    {
        if (registerObject.FactoryMethod is not null)
        {
            return registerObject.FactoryMethod.DynamicInvoke(ResolveParameters(registerObject.FactoryMethodParameterInfos))!;
        }
        else
        {
            var typeResolveInfo = registerObject.GetTypeResolveInfo(type);
            return typeResolveInfo.ConstructorInfo.Invoke(ResolveParameters(typeResolveInfo.ParameterInfos));
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