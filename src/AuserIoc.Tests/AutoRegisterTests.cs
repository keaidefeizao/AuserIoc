using AuserIoc.Common.Attributes;
using AuserIoc.Exceptions;
using FluentAssertions;
using System.Reflection;

namespace AuserIoc.Tests;

[TestClass]
public class AutoRegisterTests
{
    public interface A
    { }

    public interface B
    { }

    [Singleton]
    private class AutoRegisterInterfaceClass : A
    {
    }

    private class TestClass
    {
        private string GetName()
        {
            return "name";
        }
    }

    [Singleton]
    private class ImpTestClass : TestClass
    {
        private string Name => "name";
    }

    [TestMethod]
    public void SingletonTest()
    {
        var builder = new IocContainerBuilder();

        builder.AutoRegister([Assembly.GetExecutingAssembly()]);

        var container = builder.Build();

        var a = container.Resolve<A>();

        var testClass = container.Resolve<TestClass>();

        a.Should().NotBeNull();
        a.Should().BeOfType<AutoRegisterInterfaceClass>();

        testClass.Should().NotBeNull();
        testClass.Should().BeOfType<ImpTestClass>();
    }

    #region Test classes for different lifecycle attributes

    [Singleton]
    private class SingletonService : ISingletonService
    {
        public string Name => "SingletonService";
    }

    public interface ISingletonService
    {
        string Name { get; }
    }

    [PerDependency]
    private class PerDependencyService : IPerDependencyService
    {
        public string Name => "PerDependencyService";
    }

    public interface IPerDependencyService
    {
        string Name { get; }
    }

    [ContainerScope]
    private class ContainerScopeService : IContainerScopeService
    {
        public string Name => "ContainerScopeService";
    }

    public interface IContainerScopeService
    {
        string Name { get; }
    }

    #endregion

    [TestMethod]
    public void AutoRegister_PermDependencyAttribute_ShouldResolveAsTransient()
    {
        var builder = new IocContainerBuilder();

        builder.AutoRegister([Assembly.GetExecutingAssembly()]);

        var container = builder.Build();

        var service1 = container.Resolve<IPerDependencyService>();
        var service2 = container.Resolve<IPerDependencyService>();

        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Name.Should().Be("PerDependencyService");
        service1.Should().NotBeSameAs(service2);
    }

    [TestMethod]
    public void AutoRegister_ContainerScopeAttribute_ShouldResolveAsScoped()
    {
        var builder = new IocContainerBuilder();

        builder.AutoRegister([Assembly.GetExecutingAssembly()]);

        var container = builder.Build();

        var service1 = container.Resolve<IContainerScopeService>();
        var service2 = container.Resolve<IContainerScopeService>();

        service1.Should().BeSameAs(service2);
        service1.Name.Should().Be("ContainerScopeService");

        var scopeContainer = container.BeginContainerScope();
        var service3 = scopeContainer.Resolve<IContainerScopeService>();
        var service4 = scopeContainer.Resolve<IContainerScopeService>();

        service3.Should().BeSameAs(service4);
        service1.Should().NotBeSameAs(service3);
    }

    [TestMethod]
    public void AutoRegister_SingletonAttribute_ShouldResolveAsSingleton()
    {
        var builder = new IocContainerBuilder();

        builder.AutoRegister([Assembly.GetExecutingAssembly()]);

        var container = builder.Build();

        var service1 = container.Resolve<ISingletonService>();
        var service2 = container.Resolve<ISingletonService>();

        service1.Should().BeSameAs(service2);
        service1.Name.Should().Be("SingletonService");

        var scopeContainer = container.BeginContainerScope();
        var service3 = scopeContainer.Resolve<ISingletonService>();

        service1.Should().BeSameAs(service3);
    }

    [TestMethod]
    public void AutoRegister_TypeWithoutAttribute_ShouldNotRegister()
    {
        var builder = new IocContainerBuilder();

        // This test verifies that types without [AutoRegister] attribute are not registered
        builder.AutoRegister([Assembly.GetExecutingAssembly()]);

        var container = builder.Build();

        // Try to resolve a type that has no [AutoRegister] attribute
        // ClassWithNoAutoRegisterAttribute is a private class without any lifecycle attribute
        var action = () => container.Resolve<ClassWithNoAutoRegisterAttribute>();

        action.Should().Throw<NotRegisterTypeException>();
    }

    /// <summary>
    /// This class has NO [AutoRegister] attribute - used for testing
    /// </summary>
    private class ClassWithNoAutoRegisterAttribute
    {
    }
}