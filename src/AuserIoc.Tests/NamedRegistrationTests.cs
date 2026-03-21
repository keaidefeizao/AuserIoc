using AuserIoc.Exceptions;
using FluentAssertions;

namespace AuserIoc.Tests;

[TestClass]
public class NamedRegistrationTests
{
    #region 测试类

    interface IService
    {
        string Name { get; }
    }

    class ServiceA : IService
    {
        public string Name => "ServiceA";
    }

    class ServiceB : IService
    {
        public string Name => "ServiceB";
    }

    class ServiceC : IService
    {
        public string Name => "ServiceC";
    }

    #endregion

    [TestMethod]
    public void RegisterType_WithName_ShouldResolveByName()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<IService, ServiceA>("ServiceA");
        builder.RegisterType<IService, ServiceB>("ServiceB");

        var container = builder.Build();

        var serviceA = container.Resolve<IService>("ServiceA");
        var serviceB = container.Resolve<IService>("ServiceB");

        serviceA.Should().NotBeNull();
        serviceB.Should().NotBeNull();
        serviceA.Should().BeOfType<ServiceA>();
        serviceB.Should().BeOfType<ServiceB>();
    }

    [TestMethod]
    public void RegisterType_WithName_DifferentInstancesForDifferentNames()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<IService, ServiceA>("A");
        builder.RegisterType<IService, ServiceB>("B");

        var container = builder.Build();

        var serviceA = container.Resolve<IService>("A");
        var serviceB = container.Resolve<IService>("B");

        serviceA.Should().NotBeSameAs(serviceB);
    }

    [TestMethod]
    public void RegisterType_WithSameNameTwice_SecondRegistrationWins()
    {
        var builder = new IocContainerBuilder();

        // 不同类型可以使用相同名称注册，后面的会覆盖前面的
        builder.RegisterType<ServiceA>("Same");

        var container = builder.Build();

        // 应该能够解析第一个注册的
        var service = container.Resolve<ServiceA>("Same");

        service.Should().NotBeNull();
        service.Should().BeOfType<ServiceA>();
    }

    [TestMethod]
    public void Resolve_NonExistentName_ShouldThrowException()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<IService, ServiceA>("ServiceA");

        var container = builder.Build();

        var action = () => container.Resolve<IService>("NonExistent");

        action.Should().Throw<IocResolveException>()
            .WithMessage("*name*NonExistent*not found*");
    }

    [TestMethod]
    public void RegisterSingleton_WithName_ShouldReturnSameInstance()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IService, ServiceA>("SingletonA");

        var container = builder.Build();

        var service1 = container.Resolve<IService>("SingletonA");
        var service2 = container.Resolve<IService>("SingletonA");

        service1.Should().BeSameAs(service2);
    }

    [TestMethod]
    public void RegisterScoped_WithName_ShouldReturnSameInstanceInScope()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterScoped<IService, ServiceA>("ScopedA");

        var container = builder.Build();

        var service1 = container.Resolve<IService>("ScopedA");
        var service2 = container.Resolve<IService>("ScopedA");

        service1.Should().BeSameAs(service2);
    }

    [TestMethod]
    public void RegisterScoped_WithName_DifferentInstancesAcrossScopes()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterScoped<IService, ServiceA>("ScopedA");

        var container = builder.Build();
        var scope = container.BeginContainerScope();

        var service1 = container.Resolve<IService>("ScopedA");
        var service2 = scope.Resolve<IService>("ScopedA");

        service1.Should().NotBeSameAs(service2);
    }

    [TestMethod]
    public void RegisterInstance_WithName_ShouldResolveByName()
    {
        var builder = new IocContainerBuilder();

        var instanceA = new ServiceA();
        var instanceB = new ServiceB();

        builder.RegisterInstance(instanceA);
        builder.RegisterInstance(instanceB);

        var container = builder.Build();

        var resolvedA = container.Resolve<ServiceA>();
        var resolvedB = container.Resolve<ServiceB>();

        resolvedA.Should().BeSameAs(instanceA);
        resolvedB.Should().BeSameAs(instanceB);
    }

    [TestMethod]
    public void RegisterType_GenericType_WithName_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType(typeof(IService), typeof(ServiceA), "GenericA");

        var container = builder.Build();

        var service = container.Resolve<IService>("GenericA");

        service.Should().BeOfType<ServiceA>();
    }

    [TestMethod]
    public void RegisterType_WithoutName_AfterNamedRegistration_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<IService, ServiceA>("Named");
        builder.RegisterType<ServiceC>();

        var container = builder.Build();

        var namedService = container.Resolve<IService>("Named");
        var unnamedService = container.Resolve<ServiceC>();

        namedService.Should().BeOfType<ServiceA>();
        unnamedService.Should().BeOfType<ServiceC>();
    }

    [TestMethod]
    public void MultipleNamedRegistrations_ShouldResolveCorrectly()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IService, ServiceA>("A");
        builder.RegisterSingleton<IService, ServiceB>("B");
        builder.RegisterSingleton<IService, ServiceC>("C");

        var container = builder.Build();

        var serviceA = container.Resolve<IService>("A");
        var serviceB = container.Resolve<IService>("B");
        var serviceC = container.Resolve<IService>("C");

        serviceA.Should().BeOfType<ServiceA>();
        serviceB.Should().BeOfType<ServiceB>();
        serviceC.Should().BeOfType<ServiceC>();

        serviceA.Should().NotBeSameAs(serviceB);
        serviceB.Should().NotBeSameAs(serviceC);
        serviceA.Should().NotBeSameAs(serviceC);
    }
}
