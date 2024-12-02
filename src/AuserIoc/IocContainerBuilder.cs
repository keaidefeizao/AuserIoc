using System.Collections.ObjectModel;

namespace AuserIoc;

/// <summary>
/// ioc容器构建器
/// </summary>
public class IocContainerBuilder
{
    private readonly IList<Action<IocContainerBuilder>> _configurationMethods;
    private readonly IList<Action<IocContainerBuilder, IDictionary<Type, IocObject>>> _configurationFullMethods;

    private readonly IDictionary<Type, IocObject> _iocObjectMap;

    /// <summary>
    /// 实例化
    /// </summary>
    public IocContainerBuilder()
    {
        _configurationMethods = [];
        _configurationFullMethods = [];
        _iocObjectMap = new Dictionary<Type, IocObject>();
    }

    /// <summary>
    /// ioc容器构建
    /// </summary>
    /// <returns></returns>
    public IIocContainer Build()
    {
        foreach (var method in _configurationMethods)
        {
            method(this);
        }

        foreach (var method in _configurationFullMethods)
        {
            method(this, _iocObjectMap);
        }

#if NET462 || NET472 || NET481 || NET6_0
        var readonlyDictionary = new ReadOnlyDictionary<Type, IocObject>(_iocObjectMap);
#else
        var readonlyDictionary = _iocObjectMap.AsReadOnly();
#endif
        var container = new IocContainer(readonlyDictionary);

        // 默认注入当前容器
        var iocContainerType = typeof(IocContainer);

        if (!_iocObjectMap.ContainsKey(iocContainerType))
        {
            CreateRegisterType<IocContainer>()
            .As<IIocContainer>()
            .InstanceByContainerScope();
        }

        var iocContainerBuilderType = typeof(IocContainerBuilder);

        if (!_iocObjectMap.ContainsKey(iocContainerBuilderType))
        {
            var iocObject = new IocObject(this);
            _iocObjectMap.Add(iocContainerBuilderType, iocObject);
        }

        container.Initialize();

        return container;
    }

    /// <summary>
    /// 创建注册类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public IocObject CreateRegisterType<T>()
    {
        return CreateRegisterType(typeof(T));
    }

    /// <summary>
    /// 创建注册类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public IocObject CreateRegisterType(Type type)
    {
        if (_iocObjectMap.ContainsKey(type))
        {
            throw new Exception($"{type.FullName} 已经注册");
        }

        var iocObject = new IocObject(type);

        _iocObjectMap.Add(type, iocObject);

        return iocObject;
    }

    /// <summary>
    /// 创建注册实例
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <param name="instance"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public IocObject CreateRegisterInstance<TInterface>(TInterface instance)
    {
        var type = typeof(TInterface);

        if (_iocObjectMap.ContainsKey(type))
        {
            throw new Exception($"{type.FullName} 已经注册");
        }

        var iocObject = new IocObject(instance!);

        _iocObjectMap.Add(type, iocObject);

        return iocObject;
    }

    /// <summary>
    /// 添加配置
    /// </summary>
    /// <param name="configurationMethod"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public IocContainerBuilder Configuration(Action<IocContainerBuilder> configurationMethod)
    {
        if (configurationMethod is null)
        {
            throw new ArgumentNullException(nameof(configurationMethod));
        }

        _configurationMethods.Add(configurationMethod);

        return this;
    }

    /// <summary>
    /// 添加配置
    /// </summary>
    /// <param name="configurationMethod"></param>
    /// <returns></returns>
    public IocContainerBuilder Configuration(Action<IocContainerBuilder, IDictionary<Type, IocObject>> configurationMethod)
    {
        if (configurationMethod is null)
        {
            throw new ArgumentNullException(nameof(configurationMethod));
        }

        _configurationFullMethods.Add(configurationMethod);

        return this;
    }
}
