using AuserIoc.Common;
using AuserIoc.Exceptions;
using FluentAssertions;

namespace AuserIoc.Tests;

[TestClass]
public class RegisterObjectTests
{
    #region 测试类

    interface IService
    {
        string Name { get; }
    }

    class TestService : IService
    {
        public string Name => "TestService";
    }

    class AnotherService : IService
    {
        public string Name => "AnotherService";
    }

    #endregion

    [TestMethod]
    public void CreateRegisterType_GenericType_ShouldSuccess()
    {
        var builder = new IocContainerBuilder();

        var registerObject = builder.CreateRegisterType<TestService>();

        registerObject.Should().NotBeNull();
        registerObject.Type.Should().Be(typeof(TestService));
        registerObject.InstanceResolveType.Should().Be(InstanceResolveType.PerDependency);
    }

    [TestMethod]
    public void CreateRegisterType_NonGenericType_ShouldSuccess()
    {
        var builder = new IocContainerBuilder();

        var registerObject = builder.CreateRegisterType(typeof(TestService));

        registerObject.Should().NotBeNull();
        registerObject.Type.Should().Be(typeof(TestService));
    }

    [TestMethod]
    public void CreateRegisterType_DuplicateRegistration_ShouldThrowException()
    {
        var builder = new IocContainerBuilder();

        builder.CreateRegisterType<TestService>();

        var action = () => builder.CreateRegisterType<TestService>();

        action.Should().Throw<RegisteredTypeException>();
    }

    [TestMethod]
    public void CreateRegisterType_NullType_ShouldThrowException()
    {
        var builder = new IocContainerBuilder();

        var action = () => builder.CreateRegisterType(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void As_GenericType_ShouldSetParentType()
    {
        var builder = new IocContainerBuilder();

        builder.CreateRegisterType<TestService>().As<IService>();

        var container = builder.Build();

        var service = container.Resolve<IService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [TestMethod]
    public void As_Type_ShouldSetParentType()
    {
        var builder = new IocContainerBuilder();

        builder.CreateRegisterType<TestService>().As(typeof(IService));

        var container = builder.Build();

        var service = container.Resolve<IService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [TestMethod]
    public void As_StringTypeName_ShouldSetParentType()
    {
        var builder = new IocContainerBuilder();

        builder.CreateRegisterType<TestService>().As("AuserIoc.Tests.RegisterObjectTests+IService, AuserIoc.Tests");

        var container = builder.Build();

        var service = container.Resolve<IService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [TestMethod]
    public void SetName_ShouldSetName()
    {
        var builder = new IocContainerBuilder();

        builder.CreateRegisterType<TestService>().SetName("MyService");

        var container = builder.Build();

        var service = container.Resolve<TestService>("MyService");

        service.Should().NotBeNull();
    }

    [TestMethod]
    public void InstanceByPerDependency_ShouldSetResolveType()
    {
        var builder = new IocContainerBuilder();

        var registerObject = builder.CreateRegisterType<TestService>();

        registerObject.InstanceByPerDependency();

        registerObject.InstanceResolveType.Should().Be(InstanceResolveType.PerDependency);
    }

    [TestMethod]
    public void InstanceByPerDependency_WithInstanceSet_ShouldThrowException()
    {
        var builder = new IocContainerBuilder();

        var instance = new TestService();
        var registerObject = builder.CreateRegisterInstance(instance);

        var action = () => registerObject.InstanceByPerDependency();

        action.Should().Throw<IocObjectConfigurationException>();
    }

    [TestMethod]
    public void InstanceByContainerScope_ShouldSetResolveType()
    {
        var builder = new IocContainerBuilder();

        var registerObject = builder.CreateRegisterType<TestService>();

        registerObject.InstanceByContainerScope();

        registerObject.InstanceResolveType.Should().Be(InstanceResolveType.ContainerScope);
    }

    [TestMethod]
    public void InstanceBySingleton_ShouldSetResolveType()
    {
        var builder = new IocContainerBuilder();

        var registerObject = builder.CreateRegisterType<TestService>();

        registerObject.InstanceBySingleton();

        registerObject.InstanceResolveType.Should().Be(InstanceResolveType.Singleton);
    }

    [TestMethod]
    public void AddFactoryMethod_FuncWithoutParameters_ShouldWork()
    {
        var builder = new IocContainerBuilder();

        var created = false;
        builder.CreateRegisterType<TestService>()
            .AddFactoryMethod(() =>
            {
                created = true;
                return new TestService();
            });

        var container = builder.Build();

        var service = container.Resolve<TestService>();

        service.Should().NotBeNull();
        created.Should().BeTrue();
    }

    [TestMethod]
    public void AddFactoryMethod_FuncWithContainer_ShouldWork()
    {
        var builder = new IocContainerBuilder();

        builder.CreateRegisterType<TestService>()
            .AddFactoryMethod(container =>
            {
                container.Should().NotBeNull();
                return new TestService();
            });

        var container = builder.Build();

        var service = container.Resolve<TestService>();

        service.Should().NotBeNull();
    }

    [TestMethod]
    public void AddFactoryMethod_WithDependency_ShouldResolveDependencies()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IService, TestService>();

        builder.CreateRegisterType<AnotherService>()
            .AddFactoryMethod((IService service) =>
            {
                service.Should().NotBeNull();
                return new AnotherService();
            });

        var container = builder.Build();

        var anotherService = container.Resolve<AnotherService>();

        anotherService.Should().NotBeNull();
    }

    [TestMethod]
    public void FactoryMethodParameterInfos_CachedAfterFirstAccess()
    {
        var builder = new IocContainerBuilder();

        Func<string, TestService> factory = (name) => new TestService();
        var registerObject = builder.CreateRegisterType<TestService>()
            .AddFactoryMethod(factory);

        var parameters1 = registerObject.FactoryMethodParameterInfos;
        var parameters2 = registerObject.FactoryMethodParameterInfos;

        parameters1.Should().NotBeNullOrEmpty();
        parameters1.Length.Should().Be(1);
        ReferenceEquals(parameters1, parameters2).Should().BeTrue();
    }
}
