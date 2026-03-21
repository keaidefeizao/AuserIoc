using AuserIoc.Tests.TestObjects;
using FluentAssertions;

namespace AuserIoc.Tests;

[TestClass]
public class FactoryMethodTests
{
    #region 测试类

    interface IFactoryService
    {
        string CreatedBy { get; }
    }

    class FactoryService : IFactoryService
    {
        public string CreatedBy => "FactoryService";
    }

    class FactoryServiceWithDependency : IFactoryService
    {
        private readonly IService _dependency;

        public FactoryServiceWithDependency(IService dependency)
        {
            _dependency = dependency;
            DependencyValue = dependency.Value;
        }

        public string DependencyValue { get; }
        public string CreatedBy => "FactoryServiceWithDependency";
    }

    interface IService
    {
        string Value { get; }
    }

    class Service : IService
    {
        public string Value => "ServiceValue";
    }

    #endregion

    [TestMethod]
    public void RegisterType_FactoryMethodWithoutParameters_ShouldUseFactory()
    {
        var builder = new IocContainerBuilder();

        var factoryCalled = false;

        builder.RegisterType<IFactoryService>(_ =>
        {
            factoryCalled = true;
            return new FactoryService();
        });

        var container = builder.Build();

        var service = container.Resolve<IFactoryService>();

        service.Should().NotBeNull();
        factoryCalled.Should().BeTrue();
    }

    [TestMethod]
    public void RegisterType_FactoryMethodWithContainer_ShouldUseFactory()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<IFactoryService>(container =>
        {
            container.Should().NotBeNull();
            return new FactoryService();
        });

        var container = builder.Build();

        var service = container.Resolve<IFactoryService>();

        service.Should().NotBeNull();
    }

    [TestMethod]
    public void RegisterType_FactoryMethodWithDependency_ShouldResolveDependency()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IService, Service>();

        builder.RegisterType<IFactoryService>((IService dep) =>
        {
            dep.Should().NotBeNull();
            return new FactoryServiceWithDependency(dep);
        });

        var container = builder.Build();

        var service = container.Resolve<IFactoryService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<FactoryServiceWithDependency>();
        ((FactoryServiceWithDependency)service).DependencyValue.Should().Be("ServiceValue");
    }

    [TestMethod]
    public void RegisterType_FactoryMethod_MultipleDependencies_ShouldResolveAll()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IService, Service>();
        builder.RegisterSingleton<IFactoryService, FactoryService>();

        var factoryCalled = false;

        builder.RegisterType<string>(_ =>
        {
            factoryCalled = true;
            return "FactoryStringValue";
        });

        var container = builder.Build();

        var value = container.Resolve<string>();

        value.Should().Be("FactoryStringValue");
        factoryCalled.Should().BeTrue();
    }

    [TestMethod]
    public void RegisterType_FactoryMethod_WithContainerParameter_ShouldPassContainer()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<IFactoryService>(container =>
        {
            container.Should().NotBeNull();
            container.Should().BeOfType<IocContainer>();
            return new FactoryService();
        });

        var container = builder.Build();

        var service = container.Resolve<IFactoryService>();

        service.Should().NotBeNull();
    }

    [TestMethod]
    public void RegisterSingleton_FactoryMethod_ShouldReturnSameInstance()
    {
        var builder = new IocContainerBuilder();

        var factoryCallCount = 0;

        builder.RegisterSingleton<IFactoryService>(_ =>
        {
            factoryCallCount++;
            return new FactoryService();
        });

        var container = builder.Build();

        var service1 = container.Resolve<IFactoryService>();
        var service2 = container.Resolve<IFactoryService>();

        service1.Should().BeSameAs(service2);
        factoryCallCount.Should().Be(1);
    }

    [TestMethod]
    public void RegisterScoped_FactoryMethod_ShouldReturnSameInstanceInScope()
    {
        var builder = new IocContainerBuilder();

        var factoryCallCount = 0;

        builder.RegisterScoped<IFactoryService>(_ =>
        {
            factoryCallCount++;
            return new FactoryService();
        });

        var container = builder.Build();

        var service1 = container.Resolve<IFactoryService>();
        var service2 = container.Resolve<IFactoryService>();

        service1.Should().BeSameAs(service2);
        factoryCallCount.Should().Be(1);
    }

    [TestMethod]
    public void RegisterScoped_FactoryMethod_DifferentInstancesAcrossScopes()
    {
        var builder = new IocContainerBuilder();

        var factoryCallCount = 0;

        builder.RegisterScoped<IFactoryService>(_ =>
        {
            factoryCallCount++;
            return new FactoryService();
        });

        var container = builder.Build();
        var scope = container.BeginContainerScope();

        var service1 = container.Resolve<IFactoryService>();
        var service2 = scope.Resolve<IFactoryService>();

        service1.Should().NotBeSameAs(service2);
        factoryCallCount.Should().Be(2);
    }

    [TestMethod]
    public void RegisterType_FactoryMethod_ProducesNewInstancePerResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<IFactoryService>(_ => new FactoryService());

        var container = builder.Build();

        var service1 = container.Resolve<IFactoryService>();
        var service2 = container.Resolve<IFactoryService>();

        service1.Should().NotBeSameAs(service2);
    }

    [TestMethod]
    public void RegisterType_FactoryMethod_WithGenericFactory()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<IFactoryService, FactoryService>();

        builder.RegisterSingleton<IFactoryService>(container =>
        {
            var underlying = container.Resolve<FactoryService>();
            return underlying;
        });

        var container = builder.Build();

        var service = container.Resolve<IFactoryService>();

        service.Should().BeOfType<FactoryService>();
    }

    [TestMethod]
    public void RegisterType_FactoryMethod_CanAccessOtherRegisteredServices()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<Service>();

        builder.RegisterType<FactoryServiceWithDependency>(container =>
        {
            var dep = container.Resolve<Service>();
            return new FactoryServiceWithDependency(dep);
        });

        var container = builder.Build();

        var service = container.Resolve<FactoryServiceWithDependency>();

        service.Should().NotBeNull();
        service.DependencyValue.Should().Be("ServiceValue");
    }

    [TestMethod]
    public void RegisterType_DelegateFactory_WithMultipleParameters_ShouldWork()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IService, Service>();
        builder.RegisterSingleton<string>(_ => "StringValue");

        builder.RegisterType<FactoryServiceWithDependency>((IService service, string value) =>
        {
            service.Should().NotBeNull();
            value.Should().Be("StringValue");
            return new FactoryServiceWithDependency(service);
        });

        var container = builder.Build();

        var result = container.Resolve<FactoryServiceWithDependency>();

        result.Should().NotBeNull();
    }
}
