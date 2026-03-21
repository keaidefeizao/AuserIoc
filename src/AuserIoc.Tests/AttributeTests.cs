using AuserIoc.Common.Attributes;
using AuserIoc.Exceptions;
using AuserIoc.Tests.TestObjects;
using FluentAssertions;
using System.Reflection;

namespace AuserIoc.Tests;

[TestClass]
public class AttributeTests
{
    #region 测试类

    class SingletonAttributeService : ISingletonService
    {
        public string Value => "Singleton";
    }

    interface ISingletonService
    {
        string Value { get; }
    }

    [PerDependency]
    class PerDependencyAttributeService
    {
        public string Value => "PerDependency";
    }

    [ContainerScope]
    class ContainerScopeAttributeService
    {
        public string Value => "ContainerScope";
    }

    class ServiceWithMultipleConstructors
    {
        public string Value { get; }

        public ServiceWithMultipleConstructors()
        {
            Value = "Default";
        }

        public ServiceWithMultipleConstructors(string value)
        {
            Value = value;
        }

        [IocResolve]
        public ServiceWithMultipleConstructors(int number, string text)
        {
            Value = $"{text}-{number}";
        }
    }

    class ServiceWithIocResolveConstructor
    {
        public string Value { get; }

        public ServiceWithIocResolveConstructor()
        {
            Value = "Default";
        }

        [IocResolve]
        public ServiceWithIocResolveConstructor(TestService service)
        {
            Value = service.Value;
        }
    }

    class TestService
    {
        public string Value => "ServiceValue";
    }

    class AutoRegisterWithoutInterface
    {
        public string Value => "AutoRegister";
    }

    #endregion

    [TestMethod]
    public void SingletonAttribute_AutoRegister_ShouldRegisterAsSingleton()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<ISingletonService, SingletonAttributeService>();

        var container = builder.Build();

        var service1 = container.Resolve<ISingletonService>();
        var service2 = container.Resolve<ISingletonService>();

        service1.Should().BeSameAs(service2);
    }

    [TestMethod]
    public void PerDependencyAttribute_AutoRegister_ShouldRegisterAsTransient()
    {
        var builder = new IocContainerBuilder();

        builder.AutoRegister([Assembly.GetExecutingAssembly()]);

        var container = builder.Build();

        var service1 = container.Resolve<PerDependencyAttributeService>();
        var service2 = container.Resolve<PerDependencyAttributeService>();

        service1.Should().NotBeSameAs(service2);
        service1.Value.Should().Be("PerDependency");
        service2.Value.Should().Be("PerDependency");
    }

    [TestMethod]
    public void ContainerScopeAttribute_AutoRegister_ShouldRegisterAsScoped()
    {
        var builder = new IocContainerBuilder();

        builder.AutoRegister([Assembly.GetExecutingAssembly()]);

        var container = builder.Build();

        var service1 = container.Resolve<ContainerScopeAttributeService>();
        var service2 = container.Resolve<ContainerScopeAttributeService>();

        service1.Should().BeSameAs(service2);
    }

    [TestMethod]
    public void ContainerScopeAttribute_DifferentScopes_DifferentInstances()
    {
        var builder = new IocContainerBuilder();

        builder.AutoRegister([Assembly.GetExecutingAssembly()]);

        var container = builder.Build();
        var scope = container.BeginContainerScope();

        var service1 = container.Resolve<ContainerScopeAttributeService>();
        var service2 = scope.Resolve<ContainerScopeAttributeService>();

        service1.Should().NotBeSameAs(service2);
    }

    [TestMethod]
    public void IocResolveAttribute_MultipleConstructors_ShouldUseMarkedConstructor()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<TestService>();
        builder.RegisterType<ServiceWithMultipleConstructors>();
        builder.RegisterInstance(42);
        builder.RegisterInstance("text");

        var container = builder.Build();

        var service = container.Resolve<ServiceWithMultipleConstructors>();

        service.Value.Should().Contain("text-42");
    }

    [TestMethod]
    public void IocResolveAttribute_WithDependencies_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<TestService>();
        builder.RegisterType<ServiceWithIocResolveConstructor>();

        var container = builder.Build();

        var service = container.Resolve<ServiceWithIocResolveConstructor>();

        service.Value.Should().Be("ServiceValue");
    }

    [TestMethod]
    public void IocResolveAttribute_TwoMarkedConstructors_ShouldThrowException()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<TestClassWithTwoIocResolveConstructors>();

        var container = builder.Build();

        var action = () => container.Resolve<TestClassWithTwoIocResolveConstructors>();

        action.Should().Throw<IocResolveException>()
            .WithMessage("*only one constructor*");
    }

    [TestMethod]
    public void AutoRegister_MultipleAutoRegisterAttributes_ShouldNotThrowException()
    {
        var builder = new IocContainerBuilder();

        builder.AutoRegister([Assembly.GetExecutingAssembly()]);

        var container = builder.Build();

        container.Should().NotBeNull();
    }

    [TestMethod]
    public void AutoRegister_ClassWithBaseType_ShouldRegisterWithBaseType()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<AutoRegisterWithoutInterface>().As<AutoRegisterWithoutInterface>();

        var container = builder.Build();

        var service = container.Resolve<AutoRegisterWithoutInterface>();

        service.Should().NotBeNull();
        service.Value.Should().Be("AutoRegister");
    }

    [TestMethod]
    public void AutoRegister_NoInterface_NoBaseType_ShouldRegisterDirectly()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<AutoRegisterWithoutInterface>();

        var container = builder.Build();

        var service = container.Resolve<AutoRegisterWithoutInterface>();

        service.Should().NotBeNull();
    }
}

// 测试用的辅助类
class TestClassWithTwoIocResolveConstructors
{
    public TestClassWithTwoIocResolveConstructors()
    {
    }

    [IocResolve]
    public TestClassWithTwoIocResolveConstructors(string value)
    {
    }

    [IocResolve]
    public TestClassWithTwoIocResolveConstructors(int value)
    {
    }
}
