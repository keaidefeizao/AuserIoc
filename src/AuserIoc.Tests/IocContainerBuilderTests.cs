using AuserIoc.Tests.TestObjects;
using FluentAssertions;

namespace AuserIoc.Tests;

[TestClass]
public class IocContainerBuilderTests
{
    internal class BuilderTestService
    {
        internal BuilderTestService(IocContainerBuilder builder, IIocContainer container)
        {
            Builder = builder;
            Container = container;
        }

        internal IocContainerBuilder Builder { get; }

        internal IIocContainer Container { get; }
    }

    [TestMethod]
    public void BuildTest()
    {
        var builder = new IocContainerBuilder();

        //builder.CreateRegisterType<Universe>().As<IUniverse>().InstanceByPerDependency();
        //builder.CreateRegisterType<Moon>().As<IMoon>().InstanceByPerDependency();
        //builder.CreateRegisterType<Earth>().As<IEarth>().InstanceBySingleton();

        builder.RegisterSingleton<IUniverse, Universe>();
        builder.RegisterScoped<IEarth, Earth>();
        builder.RegisterType<IMoon, Moon>();

        var container = builder.Build();

        var universe = container.Resolve<IUniverse>();

        universe.Should().NotBeNull();
    }

    /// <summary>
    /// 测试是否有注册默认的相关组件
    /// </summary>
    [TestMethod]
    public void DefaultInjectionTest()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<BuilderTestService>();

        var container = builder.Build();

        var builderTestService = container.Resolve<BuilderTestService>();

        builderTestService.Should().NotBeNull();

        builderTestService.Builder.Should().NotBeNull();

        builderTestService.Container.Should().NotBeNull();
    }

    /// <summary>
    /// 配置测试
    /// </summary>
    [TestMethod]
    public void ConfigurationTest()
    {
        var builder = new IocContainerBuilder();
        builder.RegisterType<BuilderTestService>();
        builder.RegisterType<IUniverse, Universe>();

        builder.Configuration(builder =>
        {
            builder.RegisterType<IEarth, Earth>();
        });

        builder.Configuration((builder, iocTypeMap) =>
        {
            builder.RegisterType<IMoon, Moon>();
        });

        var container = builder.Build();

        var builderTestService = container.Resolve<BuilderTestService>();
        builderTestService.Should().NotBeNull();

        var universe = container.Resolve<IUniverse>();
        universe.Should().NotBeNull();
        universe.Earth.Should().NotBeNull();
        universe.Moon.Should().NotBeNull();
    }
}