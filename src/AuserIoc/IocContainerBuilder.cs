using AuserIoc.Exceptions;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace AuserIoc;

/// <summary>
/// ioc容器构建器
/// </summary>
public class IocContainerBuilder
{
    private readonly IList<Action<IocContainerBuilder>> _configurationMethods;
    private readonly IList<Action<IocContainerBuilder, IDictionary<Type, RegisterObject>>> _configurationFullMethods;
    private readonly ConcurrentDictionary<Type, RegisterObject> _registerObjectMap;

    /// <summary>
    /// 实例化
    /// </summary>
    public IocContainerBuilder()
    {
        _configurationMethods = [];
        _configurationFullMethods = [];
        _registerObjectMap = [];
    }

    /// <summary>
    /// ioc容器构建
    /// </summary>
    /// <returns></returns>
    public IIocContainer Build()
    {
        foreach (var method in _configurationMethods)
            method(this);

        foreach (var method in _configurationFullMethods)
            method(this, _registerObjectMap);

#if NET462 || NET472 || NET481 || NET6_0
        var readonlyDictionary = new ReadOnlyDictionary<Type, RegisterObject>(_registerObjectMap);
#else
        var readonlyDictionary = _registerObjectMap.AsReadOnly();
#endif
        var container = new IocContainer(readonlyDictionary);

        // 默认注入当前容器
        var iocContainerType = typeof(IocContainer);

        if (!_registerObjectMap.ContainsKey(iocContainerType))
        {
            var registerObject = new RegisterObject(iocContainerType);

            registerObject
                .As<IIocContainer>()
                .InstanceByContainerScope();

            _registerObjectMap.TryAdd(iocContainerType, registerObject);
        }

        var iocContainerBuilderType = typeof(IocContainerBuilder);

        if (!_registerObjectMap.ContainsKey(iocContainerBuilderType))
        {
            var registerObject = new RegisterObject(this);

            _registerObjectMap.TryAdd(iocContainerBuilderType, registerObject);
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
    public RegisterObject CreateRegisterType<T>()
    {
        return CreateRegisterType(typeof(T));
    }

    /// <summary>
    /// 创建注册类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public RegisterObject CreateRegisterType(Type type)
    {
        if (_registerObjectMap.ContainsKey(type))
        {
            throw new RegisteredTypeException(type);
        }

        var registerObject = new RegisterObject(type);

        _registerObjectMap.TryAdd(type, registerObject);

        return registerObject;
    }

    /// <summary>
    /// 创建注册实例
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <param name="instance"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public RegisterObject CreateRegisterInstance<TInterface>(TInterface instance)
    {
        var type = typeof(TInterface);

        if (_registerObjectMap.ContainsKey(type))
        {
            throw new RegisteredTypeException(type);
        }

        var registerObject = new RegisterObject(instance!);

        _registerObjectMap.TryAdd(type, registerObject);

        return registerObject;
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
    public IocContainerBuilder Configuration(Action<IocContainerBuilder, IDictionary<Type, RegisterObject>> configurationMethod)
    {
        if (configurationMethod is null)
        {
            throw new ArgumentNullException(nameof(configurationMethod));
        }

        _configurationFullMethods.Add(configurationMethod);

        return this;
    }
}
