using AuserIoc.Common.Attributes;
using AuserIoc.Exceptions;
using System.Reflection;

namespace AuserIoc;

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
    /// 每个都新实例
    /// </summary>
    PerDependency,

    /// <summary>
    /// 在范围内保持一个的实例
    /// </summary>
    ContainerScope,

    /// <summary>
    /// 
    /// </summary>
    None,
}

/// <summary>
/// 
/// </summary>
public class IocObject
{
    private readonly Type _type;

    private ConstructorInfo[]? _constructorInfos = null;
    private ParameterInfo[]? _parameterInfos = null;

    internal IocObject(Type type)
    {
        _type = type;
    }

    internal IocObject(string typeFullName) : this(Type.GetType(typeFullName)!)
    {
    }

    internal IocObject(object instance) : this(instance.GetType())
    {
        Instance = instance;
        InstanceResolveType = InstanceResolveType.None;
    }

    /// <summary>
    /// 实现类型
    /// </summary>
    public Type Type => _type;

    /// <summary>
    /// 父类类型
    /// </summary>
    public Type? ParentType { get; private set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    /// 工厂方法
    /// </summary>
    public Delegate? FactoryMethod { get; private set; }

    /// <summary>
    /// ioc对象实例解析类型
    /// </summary>
    public InstanceResolveType InstanceResolveType { get; private set; } = InstanceResolveType.PerDependency;

    /// <summary>
    /// 
    /// </summary>
    public ConstructorInfo[] ConstructorInfos
    {
        get
        {
            if (_constructorInfos is null)
            {
                if (FactoryMethod is not null)
                {
                    _constructorInfos = [];
                }
                else
                {
                    _constructorInfos = Type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default);

                    if (_constructorInfos.Length > 1)
                    {
                        var resolveAttributeType = typeof(IocResolveAttribute);

                        _constructorInfos = _constructorInfos
                            .Where(c => c.GetCustomAttribute(resolveAttributeType) is not null)
                            .ToArray();
                    }
                }
            }

            return _constructorInfos!;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public ParameterInfo[] ParameterInfos
    {
        get
        {
            if (_parameterInfos is null)
            {
                if (FactoryMethod is null)
                {
                    _parameterInfos = ConstructorInfos[0].GetParameters();
                }
                else
                {
                    _parameterInfos = FactoryMethod.Method.GetParameters();
                }
            }
            return _parameterInfos!;
        }
    }

    /// <summary>
    /// 实例
    /// </summary>
    public object? Instance { get; }

    /// <summary>
    /// 控制反转类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IocObject As<T>()
    {
        ParentType = typeof(T);
        return this;
    }

    /// <summary>
    /// 控制反转类型
    /// </summary>
    /// <param name="parentTypeFullName"></param>
    /// <returns></returns>
    public IocObject As(string parentTypeFullName)
    {
        ParentType = Type.GetType(parentTypeFullName);
        return this;
    }

    /// <summary>
    /// 控制反转类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public IocObject As(Type type)
    {
        ParentType = type;
        return this;
    }

    /// <summary>
    /// 设置名称
    /// 如果对象注册了多个实例的实现则通过名称取到指定的实现
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IocObject SetName(string name)
    {
        Name = name;
        return this;
    }

    /// <summary>
    /// 每个都是新实例
    /// </summary>
    /// <returns></returns>
    public IocObject InstanceByPerDependency()
    {
        if (Instance is not null)
        {
            throw new IocObjectConfigurationException($"{nameof(Instance)} 属性已经设置无法进行 {nameof(InstanceByPerDependency)} 方法");
        }

        InstanceResolveType = InstanceResolveType.PerDependency;

        return this;
    }

    /// <summary>
    /// 在一个容器范围内保持一个实例的注册
    /// </summary>
    /// <returns></returns>
    public IocObject InstanceByContainerScope()
    {
        if (Instance is not null)
        {
            throw new IocObjectConfigurationException($"{nameof(Instance)} 属性已经设置无法进行 {nameof(InstanceByContainerScope)} 方法");
        }

        InstanceResolveType = InstanceResolveType.ContainerScope;

        return this;
    }

    /// <summary>
    /// 单例注册
    /// </summary>
    /// <returns></returns>
    public IocObject InstanceBySingleton()
    {
        //if (Instance is not null)
        //{
        //    throw new IocObjectConfigurationException($"{nameof(Instance)} 属性已经设置无法进行 {nameof(InstanceBySingleton)} 方法");
        //}

        InstanceResolveType = InstanceResolveType.Singleton;

        return this;
    }

    /// <summary>
    /// 添加工厂方法
    /// </summary>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public IocObject AddFactoryMethod(Func<object> factoryMethod)
    {
        FactoryMethod = factoryMethod;
        return this;
    }

    /// <summary>
    /// 添加工厂方法
    /// </summary>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public IocObject AddFactoryMethod(Func<IIocContainer, object> factoryMethod)
    {
        FactoryMethod = factoryMethod;
        return this;
    }

    /// <summary>
    /// 添加工厂方法
    /// </summary>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public IocObject AddFactoryMethod(Delegate factoryMethod)
    {
        FactoryMethod = factoryMethod;
        return this;
    }

    /// <summary>
    /// 获取实际类型的构造函数
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="IocResolveException"></exception>
    internal virtual ConstructorInfo GetConstructorInfo(Type type)
    {
        if (ConstructorInfos.Length == 0 || ConstructorInfos.Length > 1)
        {
            throw new IocResolveException("构造函数必须有且只能有一个");
        }

        var constructor = ConstructorInfos[0];

        if (!constructor.ContainsGenericParameters)
            return constructor;

        var specificType = Type.MakeGenericType(type.GetGenericArguments());

        if (IocContext.ACTUAL_TYPE_CONSTRUCTORINFO_MAP.TryGetValue(specificType, out ConstructorInfo? constructorInfo))
        {
            return constructorInfo;
        }

        var constructorInfos = specificType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default);

        if (constructorInfos.Length == 0)
        {
            throw new IocResolveException("构造函数必须有且只能有一个");
        }

        if (constructorInfos.Length > 1)
        {
            var resolveAttributeType = typeof(IocResolveAttribute);

            constructorInfos = constructorInfos
                .Where(c => c.GetCustomAttribute(resolveAttributeType) is not null)
                .ToArray();

            if (constructorInfos.Length > 1)
            {
                throw new IocResolveException("构造函数必须有且只能有一个");
            }
        }

        IocContext.ACTUAL_TYPE_CONSTRUCTORINFO_MAP.TryAdd(specificType, constructorInfos[0]);

        return constructorInfos[0];
    }
}
