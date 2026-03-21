using AuserIoc.Common.Attributes;
using AuserIoc.Tests.TestObjects;
using FluentAssertions;

namespace AuserIoc.Tests;

[TestClass]
public class LifecycleTests
{
    #region 测试类

    interface ILifecycleService
    {
        string Id { get; }
    }

    class LifecycleService : ILifecycleService
    {
        public string Id { get; } = Guid.NewGuid().ToString();
    }

    class AnotherLifecycleService : ILifecycleService
    {
        public string Id { get; } = Guid.NewGuid().ToString();
    }

    class YetAnotherLifecycleService : ILifecycleService
    {
        public string Id { get; } = Guid.NewGuid().ToString();
    }

    interface IService
    {
        Earth Earth { get; }
        Moon Moon { get; }
    }

    class Service : IService
    {
        public Earth Earth => new Earth();
        public Moon Moon => new Moon();
        public string Value => "ServiceValue";
    }

    class ServiceWithServiceDependency
    {
        public IService Service { get; }

        public ServiceWithServiceDependency(IService service)
        {
            Service = service;
        }
    }

    class AnotherService : IService
    {
        public Earth Earth => null!;
        public Moon Moon => null!;
    }

    class YetAnotherService : IService
    {
        public Earth Earth => null!;
        public Moon Moon => null!;
    }

    #endregion

    [TestMethod]
    public void MixedLifecycles_SingletonAndTransient_ShouldBehaveCorrectly()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<ILifecycleService, LifecycleService>("Singleton");
        builder.RegisterType<ILifecycleService, AnotherLifecycleService>("Transient");

        var container = builder.Build();

        var singleton1 = container.Resolve<ILifecycleService>("Singleton");
        var singleton2 = container.Resolve<ILifecycleService>("Singleton");
        var transient1 = container.Resolve<ILifecycleService>("Transient");
        var transient2 = container.Resolve<ILifecycleService>("Transient");

        singleton1.Should().BeSameAs(singleton2);
        transient1.Should().NotBeSameAs(transient2);
    }

    [TestMethod]
    public void MixedLifecycles_SingletonAndScoped_ShouldBehaveCorrectly()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<ILifecycleService, LifecycleService>("Singleton");
        builder.RegisterScoped<ILifecycleService, AnotherLifecycleService>("Scoped");

        var container = builder.Build();

        var singleton1 = container.Resolve<ILifecycleService>("Singleton");
        var singleton2 = container.Resolve<ILifecycleService>("Singleton");
        var scoped1 = container.Resolve<ILifecycleService>("Scoped");
        var scoped2 = container.Resolve<ILifecycleService>("Scoped");

        singleton1.Should().BeSameAs(singleton2);
        scoped1.Should().BeSameAs(scoped2);
    }

    [TestMethod]
    public void MixedLifecycles_AcrossContainerScopes()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<ILifecycleService, LifecycleService>("Singleton");
        builder.RegisterScoped<ILifecycleService, AnotherLifecycleService>("Scoped");
        builder.RegisterType<ILifecycleService, YetAnotherLifecycleService>("Transient");

        var container = builder.Build();
        var scope = container.BeginContainerScope();

        var singleton1 = container.Resolve<ILifecycleService>("Singleton");
        var singleton2 = scope.Resolve<ILifecycleService>("Singleton");
        singleton1.Should().BeSameAs(singleton2);

        var scoped1 = container.Resolve<ILifecycleService>("Scoped");
        var scoped2 = scope.Resolve<ILifecycleService>("Scoped");
        scoped1.Should().NotBeSameAs(scoped2);

        var transient1 = container.Resolve<ILifecycleService>("Transient");
        var transient2 = scope.Resolve<ILifecycleService>("Transient");
        transient1.Should().NotBeSameAs(transient2);
    }

    [TestMethod]
    public void MixedLifecycles_ScopedWithinSameContainer()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterScoped<ILifecycleService, LifecycleService>();

        var container = builder.Build();

        var scoped1 = container.Resolve<ILifecycleService>();
        var scoped2 = container.Resolve<ILifecycleService>();

        scoped1.Should().BeSameAs(scoped2);
    }

    [TestMethod]
    public void MixedLifecycles_NestedScopes()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterScoped<ILifecycleService, LifecycleService>();

        var container = builder.Build();
        var scope1 = container.BeginContainerScope();
        var scope2 = scope1.BeginContainerScope();

        var scoped1 = scope1.Resolve<ILifecycleService>();
        var scoped2 = scope2.Resolve<ILifecycleService>();

        scoped1.Should().NotBeSameAs(scoped2);
    }

    [TestMethod]
    public void MixedLifecycles_SingletonResolvedInScope_SharedAcrossScopes()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<ILifecycleService, LifecycleService>();

        var container = builder.Build();
        var scope1 = container.BeginContainerScope();
        var scope2 = container.BeginContainerScope();

        var scoped1 = scope1.Resolve<ILifecycleService>();
        var scoped2 = scope2.Resolve<ILifecycleService>();

        scoped1.Should().BeSameAs(scoped2);
    }

    [TestMethod]
    public void MixedLifecycles_TransientDependsOnSingleton()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<ILifecycleService, LifecycleService>("Singleton");
        builder.RegisterType<ILifecycleService, AnotherLifecycleService>("Transient");

        var container = builder.Build();

        var transient1 = container.Resolve<ILifecycleService>("Transient");
        var transient2 = container.Resolve<ILifecycleService>("Transient");

        transient1.Should().NotBeSameAs(transient2);
    }

    [TestMethod]
    public void MixedLifecycles_SingletonDependsOnTransient()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<IService, Service>();
        builder.RegisterSingleton<ServiceWithServiceDependency>();

        var container = builder.Build();

        var singleton1 = container.Resolve<ServiceWithServiceDependency>();
        var singleton2 = container.Resolve<ServiceWithServiceDependency>();

        singleton1.Should().BeSameAs(singleton2);
        singleton1.Service.Should().NotBeNull();
    }

    [TestMethod]
    public void MixedLifecycles_ScopedDependsOnSingleton()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IService, Service>();
        builder.RegisterScoped<ServiceWithServiceDependency>();

        var container = builder.Build();

        var scoped1 = container.Resolve<ServiceWithServiceDependency>();
        var scoped2 = container.Resolve<ServiceWithServiceDependency>();

        scoped1.Should().BeSameAs(scoped2);
        scoped1.Service.Should().NotBeNull();
    }

    [TestMethod]
    public void MixedLifecycles_DisposeScope_ClearsScopedInstances()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterScoped<ILifecycleService, LifecycleService>();

        var container = builder.Build();

        var scoped1 = container.Resolve<ILifecycleService>();

        using (var scope = container.BeginContainerScope())
        {
            var scopedInScope = scope.Resolve<ILifecycleService>();
            scopedInScope.Should().NotBeSameAs(scoped1);
        }

        var scoped2 = container.Resolve<ILifecycleService>();
        scoped2.Should().BeSameAs(scoped1);
    }

    [TestMethod]
    public void MixedLifecycles_ComplexDependencyChain()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IService, Service>();
        builder.RegisterScoped<IService, AnotherService>("Scoped");
        builder.RegisterType<IService, YetAnotherService>("Transient");

        var container = builder.Build();

        var singleton = container.Resolve<IService>();
        var scoped1 = container.Resolve<IService>("Scoped");
        var scoped2 = container.Resolve<IService>("Scoped");
        var transient1 = container.Resolve<IService>("Transient");
        var transient2 = container.Resolve<IService>("Transient");

        singleton.Should().BeOfType<Service>();
        scoped1.Should().BeOfType<AnotherService>();
        scoped2.Should().BeOfType<AnotherService>();
        scoped1.Should().BeSameAs(scoped2);
        transient1.Should().BeOfType<YetAnotherService>();
        transient2.Should().BeOfType<YetAnotherService>();
        transient1.Should().NotBeSameAs(transient2);
    }
}
