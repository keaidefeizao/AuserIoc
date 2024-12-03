using AuserIoc.Common;
using AuserIoc.Common.Attributes;
using AuserIoc.Exceptions;
using System.Reflection;

namespace AuserIoc;

/// <summary>
/// 注册对象
/// </summary>
public class RegisterObject
{
    private readonly Type _type;

    private ConstructorInfo[]? _constructorInfos = null;
    private ParameterInfo[]? _parameterInfos = null;

    internal RegisterObject(Type type)
    {
        _type = type;
    }

    internal RegisterObject(string typeFullName) : this(Type.GetType(typeFullName)!)
    {
    }

    internal RegisterObject(object instance) : this(instance.GetType())
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
    /// 工厂方法的参数信息
    /// </summary>
    public ParameterInfo[] FactoryMethodParameterInfos
    {
        get
        {
            if (_parameterInfos is null)
            {
                if (FactoryMethod is null)
                {
                    _parameterInfos = [];
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
    public RegisterObject As<T>()
    {
        ParentType = typeof(T);
        return this;
    }

    /// <summary>
    /// 控制反转类型
    /// </summary>
    /// <param name="parentTypeFullName"></param>
    /// <returns></returns>
    public RegisterObject As(string parentTypeFullName)
    {
        ParentType = Type.GetType(parentTypeFullName);
        return this;
    }

    /// <summary>
    /// 控制反转类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public RegisterObject As(Type type)
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
    public RegisterObject SetName(string name)
    {
        Name = name;
        return this;
    }

    /// <summary>
    /// 每个都是新实例
    /// </summary>
    /// <returns></returns>
    public RegisterObject InstanceByPerDependency()
    {
        if (Instance is not null)
        {
            //throw new IocObjectConfigurationException($"{nameof(Instance)} 属性已经设置无法进行 {nameof(InstanceByPerDependency)} 方法");
            throw new IocObjectConfigurationException($"The {nameof(Instance)} property is already set. The {nameof(InstanceByPerDependency)} method cannot be performed");
        }

        InstanceResolveType = InstanceResolveType.PerDependency;

        return this;
    }

    /// <summary>
    /// 在一个容器范围内保持一个实例的注册
    /// </summary>
    /// <returns></returns>
    public RegisterObject InstanceByContainerScope()
    {
        if (Instance is not null)
        {
            throw new IocObjectConfigurationException($"The {nameof(Instance)} property is already set. The {nameof(InstanceByContainerScope)} method cannot be performed");
        }

        InstanceResolveType = InstanceResolveType.ContainerScope;

        return this;
    }

    /// <summary>
    /// 单例注册
    /// </summary>
    /// <returns></returns>
    public RegisterObject InstanceBySingleton()
    {
        InstanceResolveType = InstanceResolveType.Singleton;
        return this;
    }

    /// <summary>
    /// 添加工厂方法
    /// </summary>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public RegisterObject AddFactoryMethod(Func<object> factoryMethod)
    {
        FactoryMethod = factoryMethod;
        return this;
    }

    /// <summary>
    /// 添加工厂方法
    /// </summary>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public RegisterObject AddFactoryMethod(Func<IIocContainer, object> factoryMethod)
    {
        FactoryMethod = factoryMethod;
        return this;
    }

    /// <summary>
    /// 添加工厂方法
    /// </summary>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public RegisterObject AddFactoryMethod(Delegate factoryMethod)
    {
        FactoryMethod = factoryMethod;
        return this;
    }

    /// <summary>
    /// 获取实际类型的构造函数相关信息
    /// </summary>
    /// <param name="type"></param>
    /// <returns>类型解析的信息</returns>
    /// <exception cref="IocResolveException"></exception>
    internal virtual TypeResolveInfo GetTypeResolveInfo(Type type)
    {
        if (IocContext.ACTUAL_TYPE_TYPERESOLVEINFO_MAP.TryGetValue(type, out TypeResolveInfo? typeResolveInfo))
        {
            return typeResolveInfo;
        }

        ConstructorInfo[] constructorInfos;

        if (type.IsGenericType)
        {
            var specificType = Type.MakeGenericType(type.GetGenericArguments());
            constructorInfos = specificType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default);
        }
        else
        {
            constructorInfos = Type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default);
        }

        if (constructorInfos.Length == 0)
        {
            throw new IocResolveException("There must be only one constructor");
        }

        if (constructorInfos.Length > 1)
        {
            var resolveAttributeType = typeof(IocResolveAttribute);

            constructorInfos = constructorInfos
                .Where(c => c.GetCustomAttribute(resolveAttributeType) is not null)
                .ToArray();

            if (constructorInfos.Length > 1)
            {
                throw new IocResolveException("There must be only one constructor");
            }
        }

        var constructorInfo = constructorInfos[0];

        typeResolveInfo = new TypeResolveInfo(type, constructorInfo, constructorInfo.GetParameters());

        IocContext.ACTUAL_TYPE_TYPERESOLVEINFO_MAP.TryAdd(type, typeResolveInfo);

        return typeResolveInfo;
    }
}
