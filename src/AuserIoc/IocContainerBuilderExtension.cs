using AuserIoc.Common.Attributes;
using AuserIoc.Exceptions;
using System.Reflection;

namespace AuserIoc;

/// <summary>
/// IocContainerBuilder的扩展函数
/// </summary>
public static class IocContainerBuilderExtension
{
    /// <summary>
    /// 注册类型，解析类型为瞬时的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterType<TFrom, TTo>(this IocContainerBuilder iocContainerBuilder, string name)
    {
        iocContainerBuilder.CreateRegisterType<TTo>().As<TFrom>().SetName(name).InstanceByPerDependency();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为瞬时的
    /// </summary>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterType(this IocContainerBuilder iocContainerBuilder, Type fromType, Type toType, string name)
    {
        iocContainerBuilder.CreateRegisterType(toType).As(fromType).SetName(name).InstanceByPerDependency();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为瞬时的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterType<T>(this IocContainerBuilder iocContainerBuilder, string name)
    {
        iocContainerBuilder.CreateRegisterType<T>().SetName(name).InstanceByPerDependency();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为瞬时的
    /// </summary>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterType(this IocContainerBuilder iocContainerBuilder, Type type, string name)
    {
        iocContainerBuilder.CreateRegisterType(type).SetName(name).InstanceByPerDependency();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为瞬时的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterType<TFrom, TTo>(this IocContainerBuilder iocContainerBuilder)
    {
        iocContainerBuilder.CreateRegisterType<TTo>().As<TFrom>().InstanceByPerDependency();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为瞬时的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterType<T>(this IocContainerBuilder iocContainerBuilder)
    {
        iocContainerBuilder.CreateRegisterType<T>().InstanceByPerDependency();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为瞬时的
    /// </summary>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterType(this IocContainerBuilder iocContainerBuilder, Type fromType, Type toType)
    {
        iocContainerBuilder.CreateRegisterType(toType).As(fromType).InstanceByPerDependency();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为瞬时的
    /// </summary>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterType(this IocContainerBuilder iocContainerBuilder, Type type)
    {
        iocContainerBuilder.CreateRegisterType(type).InstanceByPerDependency();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为瞬时的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterType<TFrom>(this IocContainerBuilder iocContainerBuilder, Func<TFrom> factoryMethod)
    {
        iocContainerBuilder.CreateRegisterType<TFrom>().AddFactoryMethod(factoryMethod).InstanceByPerDependency();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为瞬时的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterType<TFrom>(this IocContainerBuilder iocContainerBuilder, Func<IIocContainer, TFrom> factoryMethod)
    {
        iocContainerBuilder.CreateRegisterType<TFrom>().AddFactoryMethod(factoryMethod).InstanceByPerDependency();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为瞬时的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterType<TFrom>(this IocContainerBuilder iocContainerBuilder, Delegate factoryMethod)
    {
        iocContainerBuilder.CreateRegisterType<TFrom>().AddFactoryMethod(factoryMethod).InstanceByPerDependency();
        return iocContainerBuilder;
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 注册类型，解析类型为容器范围内的不重复的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterScoped<TFrom, TTo>(this IocContainerBuilder iocContainerBuilder)
    {
        iocContainerBuilder.CreateRegisterType<TTo>().As<TFrom>().InstanceByContainerScope();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为容器范围内的不重复的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterScoped<TFrom, TTo>(this IocContainerBuilder iocContainerBuilder, string name)
    {
        iocContainerBuilder.CreateRegisterType<TTo>().As<TFrom>().SetName(name).InstanceByContainerScope();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为容器范围内的不重复的
    /// </summary>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterScoped(this IocContainerBuilder iocContainerBuilder, Type fromType, Type toType)
    {
        iocContainerBuilder.CreateRegisterType(toType).As(fromType).InstanceByContainerScope();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为容器范围内的不重复的
    /// </summary>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterScoped(this IocContainerBuilder iocContainerBuilder, Type fromType, Type toType, string name)
    {
        iocContainerBuilder.CreateRegisterType(toType).As(fromType).SetName(name).InstanceByContainerScope();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为容器范围内的不重复的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterScoped<T>(this IocContainerBuilder iocContainerBuilder)
    {
        iocContainerBuilder.CreateRegisterType<T>().InstanceByContainerScope();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为容器范围内的不重复的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterScoped<T>(this IocContainerBuilder iocContainerBuilder, string name)
    {
        iocContainerBuilder.CreateRegisterType<T>().SetName(name).InstanceByContainerScope();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为容器范围内的不重复的
    /// </summary>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterScoped(this IocContainerBuilder iocContainerBuilder, Type type)
    {
        iocContainerBuilder.CreateRegisterType(type).InstanceByContainerScope();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为容器范围内的不重复的
    /// </summary>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterScoped(this IocContainerBuilder iocContainerBuilder, Type type, string name)
    {
        iocContainerBuilder.CreateRegisterType(type).SetName(name).InstanceByContainerScope();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为容器范围内的不重复的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterScoped<TFrom>(this IocContainerBuilder iocContainerBuilder, Func<TFrom> factoryMethod)
    {
        iocContainerBuilder.CreateRegisterType<TFrom>().AddFactoryMethod(factoryMethod).InstanceByContainerScope();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为容器范围内的不重复的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterScoped<TFrom>(this IocContainerBuilder iocContainerBuilder, Func<IIocContainer, TFrom> factoryMethod)
    {
        iocContainerBuilder.CreateRegisterType<TFrom>().AddFactoryMethod(factoryMethod).InstanceByContainerScope();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为容器范围内的不重复的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterScoped<TFrom>(this IocContainerBuilder iocContainerBuilder, Delegate factoryMethod)
    {
        iocContainerBuilder.CreateRegisterType<TFrom>().AddFactoryMethod(factoryMethod).InstanceByContainerScope();
        return iocContainerBuilder;
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 注册类型，解析类型为单例的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterSingleton<TFrom, TTo>(this IocContainerBuilder iocContainerBuilder)
    {
        iocContainerBuilder.CreateRegisterType<TTo>().As<TFrom>().InstanceBySingleton();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为单例的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterSingleton<TFrom, TTo>(this IocContainerBuilder iocContainerBuilder, string name)
    {
        iocContainerBuilder.CreateRegisterType<TTo>().As<TFrom>().SetName(name).InstanceBySingleton();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为单例的
    /// </summary>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterSingleton(this IocContainerBuilder iocContainerBuilder, Type fromType, Type toType)
    {
        iocContainerBuilder.CreateRegisterType(toType).As(fromType).InstanceBySingleton();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为单例的
    /// </summary>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterSingleton(this IocContainerBuilder iocContainerBuilder, Type fromType, Type toType, string name)
    {
        iocContainerBuilder.CreateRegisterType(toType).As(fromType).SetName(name).InstanceBySingleton();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为单例的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterSingleton<T>(this IocContainerBuilder iocContainerBuilder)
    {
        iocContainerBuilder.CreateRegisterType<T>().InstanceBySingleton();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为单例的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterSingleton<T>(this IocContainerBuilder iocContainerBuilder, string name)
    {
        iocContainerBuilder.CreateRegisterType<T>().SetName(name).InstanceBySingleton();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为单例的
    /// </summary>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterSingleton(this IocContainerBuilder iocContainerBuilder, Type type)
    {
        iocContainerBuilder.CreateRegisterType(type).InstanceBySingleton();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为单例的
    /// </summary>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterSingleton(this IocContainerBuilder iocContainerBuilder, Type type, string name)
    {
        iocContainerBuilder.CreateRegisterType(type).SetName(name).InstanceBySingleton();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为单例的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterSingleton<TFrom>(this IocContainerBuilder iocContainerBuilder, Func<TFrom> factoryMethod)
    {
        iocContainerBuilder.CreateRegisterType<TFrom>().AddFactoryMethod(factoryMethod).InstanceBySingleton();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为单例的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterSingleton<TFrom>(this IocContainerBuilder iocContainerBuilder, Func<IIocContainer, TFrom> factoryMethod)
    {
        iocContainerBuilder.CreateRegisterType<TFrom>().AddFactoryMethod(factoryMethod).InstanceBySingleton();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册类型，解析类型为单例的
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="factoryMethod"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterSingleton<TFrom>(this IocContainerBuilder iocContainerBuilder, Delegate factoryMethod)
    {
        iocContainerBuilder.CreateRegisterType<TFrom>().AddFactoryMethod(factoryMethod).InstanceBySingleton();
        return iocContainerBuilder;
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 注册实例
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterInstance<TInterface>(this IocContainerBuilder iocContainerBuilder, TInterface instance)
    {
        iocContainerBuilder.CreateRegisterInstance(instance).InstanceBySingleton();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册实例
    /// </summary>
    /// <typeparam name="IFrom"></typeparam>
    /// <typeparam name="TInterface"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterInstance<IFrom, TInterface>(this IocContainerBuilder iocContainerBuilder, TInterface instance)
    {
        iocContainerBuilder.CreateRegisterInstance(instance).As<IFrom>().InstanceBySingleton();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册实例
    /// </summary>
    /// <typeparam name="IFrom"></typeparam>
    /// <typeparam name="TInterface"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="instance"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterInstance<IFrom, TInterface>(this IocContainerBuilder iocContainerBuilder, TInterface instance, string name)
    {
        iocContainerBuilder.CreateRegisterInstance(instance).As<IFrom>().SetName(name).InstanceBySingleton();
        return iocContainerBuilder;
    }

    /// <summary>
    /// 注册实例
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="instance"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IocContainerBuilder RegisterInstance<TInterface>(this IocContainerBuilder iocContainerBuilder, TInterface instance, string name)
    {
        iocContainerBuilder.CreateRegisterInstance(instance).SetName(name).InstanceBySingleton();
        return iocContainerBuilder;
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 自动注册
    /// 根据类标注特性进行注册
    /// </summary>
    /// <param name="iocContainerBuilder"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IocContainerBuilder AutoRegister(this IocContainerBuilder iocContainerBuilder, IEnumerable<Assembly> assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes().Where(t=>t.IsClass && !t.IsAbstract))
            {
                var autoRegisterAttributes = type.GetCustomAttributes<AutoRegisterAttribute>().ToArray();

                if (autoRegisterAttributes is null || autoRegisterAttributes.Length < 1)
                    continue;

                if (autoRegisterAttributes.Length > 1)
                {
                    throw new AutoRegisterException(type, $"Only one '{nameof(AutoRegisterAttribute)}' can be annotated");
                }

                var ro = iocContainerBuilder.CreateRegisterType(type);

                var interfaceTypes = type.GetInterfaces();

                if (interfaceTypes.Length > 1)
                {
                    throw new UnableToDetermineInterfaceException(type);
                }
                else if (interfaceTypes.Length == 1)
                {
                    ro.As(interfaceTypes[0]);
                }
                else
                {
                    if (type.BaseType is not null && type.BaseType.FullName != "System.Object")
                    {
                        ro.As(type.BaseType);
                    }
                }

                var autoRegisterAttribute = autoRegisterAttributes[0];

                if (autoRegisterAttribute is SingletonAttribute)
                {
                    ro.InstanceBySingleton();
                }
                else if(autoRegisterAttribute is PerDependencyAttribute)
                {
                    ro.InstanceByPerDependency();
                }
                else if (autoRegisterAttribute is ContainerScopeAttribute)
                {
                    ro.InstanceByContainerScope();
                }
            }
        }

        return iocContainerBuilder;
    }
}
