using AuserIoc.Exceptions;
using AuserIoc.Tests.TestObjects;
using FluentAssertions;

namespace AuserIoc.Tests;

[TestClass]
public class InstanceRegistrationTests
{
    #region 测试类

    interface IInstanceService
    {
        string Id { get; }
    }

    class InstanceService : IInstanceService
    {
        public string Id { get; } = Guid.NewGuid().ToString();
    }

    class AnotherInstanceService
    {
        public string Id { get; } = Guid.NewGuid().ToString();
    }

    #endregion

    [TestMethod]
    public void RegisterInstance_ShouldReturnSameInstance()
    {
        var builder = new IocContainerBuilder();

        var instance = new InstanceService();

        builder.RegisterInstance(instance);

        var container = builder.Build();

        var resolved = container.Resolve<InstanceService>();

        resolved.Should().BeSameAs(instance);
    }

    [TestMethod]
    public void RegisterInstance_AsInterface_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        IInstanceService instance = new InstanceService();

        builder.RegisterInstance<IInstanceService>(instance);

        var container = builder.Build();

        var resolved = container.Resolve<IInstanceService>();

        resolved.Should().BeSameAs(instance);
    }

    [TestMethod]
    public void RegisterInstance_AsType_WithTypeOverride_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        var instance = new InstanceService();

        builder.RegisterInstance<IInstanceService, InstanceService>(instance);

        var container = builder.Build();

        var resolved = container.Resolve<IInstanceService>();

        resolved.Should().BeSameAs(instance);
    }

    [TestMethod]
    public void RegisterInstance_WithMultipleTypes_ShouldResolveAll()
    {
        var builder = new IocContainerBuilder();

        var service1 = new InstanceService();
        var service2 = new AnotherInstanceService();

        builder.RegisterInstance(service1);
        builder.RegisterInstance(service2);

        var container = builder.Build();

        var resolved1 = container.Resolve<InstanceService>();
        var resolved2 = container.Resolve<AnotherInstanceService>();

        resolved1.Should().BeSameAs(service1);
        resolved2.Should().BeSameAs(service2);
    }

    [TestMethod]
    public void RegisterInstance_NullInstance_ShouldThrowException()
    {
        var builder = new IocContainerBuilder();

        var action = () => builder.RegisterInstance<IInstanceService>(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void RegisterInstance_BehavesAsSingleton()
    {
        var builder = new IocContainerBuilder();

        var instance = new InstanceService();

        builder.RegisterInstance(instance);

        var container = builder.Build();

        var resolved1 = container.Resolve<InstanceService>();
        var resolved2 = container.Resolve<InstanceService>();

        resolved1.Should().BeSameAs(resolved2);
        resolved1.Should().BeSameAs(instance);
    }

    [TestMethod]
    public void RegisterInstance_AcrossContainerScopes_ShouldBeSameInstance()
    {
        var builder = new IocContainerBuilder();

        var instance = new InstanceService();

        builder.RegisterInstance(instance);

        var container = builder.Build();
        var scope = container.BeginContainerScope();

        var resolved1 = container.Resolve<InstanceService>();
        var resolved2 = scope.Resolve<InstanceService>();

        resolved1.Should().BeSameAs(resolved2);
        resolved1.Should().BeSameAs(instance);
    }

    [TestMethod]
    public void RegisterInstance_WithTypeParameter_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        IInstanceService instance = new InstanceService();

        builder.RegisterInstance(instance);

        var container = builder.Build();

        var resolved = container.Resolve<IInstanceService>();

        resolved.Should().BeSameAs(instance);
    }

    [TestMethod]
    public void RegisterInstance_MultipleInstances_DifferentTypes()
    {
        var builder = new IocContainerBuilder();

        var service1 = new InstanceService();
        var service2 = new InstanceService();

        builder.RegisterInstance(service1);
        builder.RegisterInstance<IInstanceService>(service2, "Named");

        var container = builder.Build();

        var resolved1 = container.Resolve<InstanceService>();
        var resolved2 = container.Resolve<IInstanceService>("Named");

        resolved1.Should().BeSameAs(service1);
        resolved2.Should().BeSameAs(service2);
        resolved1.Should().NotBeSameAs(resolved2);
    }

    [TestMethod]
    public void RegisterInstance_WithInterfaceAndName_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        IInstanceService instance = new InstanceService();

        builder.RegisterInstance<IInstanceService>(instance, "MyService");

        var container = builder.Build();

        var resolved = container.Resolve<IInstanceService>("MyService");

        resolved.Should().BeSameAs(instance);
    }

    [TestMethod]
    public void RegisterInstance_DuplicateRegistration_ShouldThrowException()
    {
        var builder = new IocContainerBuilder();

        var instance = new InstanceService();

        builder.RegisterInstance(instance);

        var action = () => builder.RegisterInstance(instance);

        action.Should().Throw<RegisteredTypeException>();
    }

    // 扩展方法测试
    [TestClass]
    public class RegisterInstanceExtensionTests
    {
        interface IService
        {
            string Value { get; }
        }

        class Service : IService
        {
            public string Value => "Service";
        }

        [TestMethod]
        public void RegisterInstance_GenericInterface_ShouldWork()
        {
            var builder = new IocContainerBuilder();
            IService instance = new Service();

            builder.RegisterInstance<IService>(instance);

            var container = builder.Build();
            var resolved = container.Resolve<IService>();

            resolved.Should().BeSameAs(instance);
        }

        [TestMethod]
        public void RegisterInstance_GenericInterfaceAndType_ShouldWork()
        {
            var builder = new IocContainerBuilder();
            var instance = new Service();

            builder.RegisterInstance<IService, Service>(instance);

            var container = builder.Build();
            var resolved = container.Resolve<IService>();

            resolved.Should().BeSameAs(instance);
        }

        [TestMethod]
        public void RegisterInstance_GenericInterfaceAndTypeWithName_ShouldWork()
        {
            var builder = new IocContainerBuilder();
            var instance = new Service();

            builder.RegisterInstance<IService, Service>(instance, "Named");

            var container = builder.Build();
            var resolved = container.Resolve<IService>("Named");

            resolved.Should().BeSameAs(instance);
        }

        [TestMethod]
        public void RegisterInstance_GenericTypeWithName_ShouldWork()
        {
            var builder = new IocContainerBuilder();
            var instance = new Service();

            builder.RegisterInstance<Service>(instance, "Named");

            var container = builder.Build();
            var resolved = container.Resolve<Service>("Named");

            resolved.Should().BeSameAs(instance);
        }
    }
}
