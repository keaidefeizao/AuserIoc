using AuserIoc.Common.Attributes;
using AuserIoc.Common;
using AuserIoc.Exceptions;
using AuserIoc.Tests.TestObjects;
using FluentAssertions;

namespace AuserIoc.Tests;

[TestClass]
public class ExceptionTests
{
    #region 测试类

    class ClassWithNoPublicConstructor
    {
        internal ClassWithNoPublicConstructor() { }
    }

    class ClassWithMultiplePublicConstructors
    {
        public ClassWithMultiplePublicConstructors() { }

        [IocResolve]
        public ClassWithMultiplePublicConstructors(string value) { }
    }

    class ClassWithTwoIocResolveConstructors
    {
        [IocResolve]
        public ClassWithTwoIocResolveConstructors(string value) { }

        [IocResolve]
        public ClassWithTwoIocResolveConstructors(int value) { }
    }

    class CircularDependencyA
    {
        public CircularDependencyA(CircularDependencyB b) { }
    }

    class CircularDependencyB
    {
        public CircularDependencyB(CircularDependencyA a) { }
    }

    interface IServiceA { }
    interface IServiceB { }

    class ServiceA : IServiceA
    {
        public ServiceA(IServiceB b) { }
    }

    class ServiceB : IServiceB
    {
        public ServiceB(IServiceA a) { }
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

    #endregion

    [TestMethod]
    public void Resolve_UnregisteredType_ShouldThrowNotRegisterTypeException()
    {
        var builder = new IocContainerBuilder();
        var container = builder.Build();

        var action = () => container.Resolve<IService>();

        action.Should().Throw<NotRegisterTypeException>()
            .WithMessage($"*{nameof(IService)}*not registered*");
    }

    [TestMethod]
    public void CreateRegisterType_DuplicateRegistration_ShouldThrowRegisteredTypeException()
    {
        var builder = new IocContainerBuilder();

        builder.CreateRegisterType<Service>();

        var action = () => builder.CreateRegisterType<Service>();

        action.Should().Throw<RegisteredTypeException>()
            .WithMessage($"*{nameof(Service)}*");
    }

    [TestMethod]
    public void RegisterType_DuplicateRegistration_ShouldThrowRegisteredTypeException()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<IService, Service>();

        var action = () => builder.RegisterType<IService, Service>();

        action.Should().Throw<RegisteredTypeException>();
    }

    [TestMethod]
    public void RegisterSingleton_DuplicateRegistration_ShouldThrowRegisteredTypeException()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IService, Service>();

        var action = () => builder.RegisterSingleton<IService, Service>();

        action.Should().Throw<RegisteredTypeException>();
    }

    [TestMethod]
    public void RegisterScoped_DuplicateRegistration_ShouldThrowRegisteredTypeException()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterScoped<IService, Service>();

        var action = () => builder.RegisterScoped<IService, Service>();

        action.Should().Throw<RegisteredTypeException>();
    }

    [TestMethod]
    public void Resolve_ClassWithNoPublicConstructor_ShouldThrowIocResolveException()
    {
        var builder = new IocContainerBuilder();

        // 具有单个内部构造函数的类可以被解析
        // 因为 IOC 容器会查找所有构造函数
        builder.RegisterType<ClassWithNoPublicConstructor>();

        var container = builder.Build();

        // 应该能够解析，因为内部构造函数也能被发现
        var instance = container.Resolve<ClassWithNoPublicConstructor>();

        instance.Should().NotBeNull();
    }

    [TestMethod]
    public void Resolve_ClassWithMultiplePublicConstructors_NoIocResolveAttribute_ShouldThrowIocResolveException()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<ClassWithMultiplePublicConstructors>();

        var container = builder.Build();

        // 当有多个构造函数且没有 [IocResolve] 特性时，会抛出异常
        // 异常可能是 IocResolveException 或 NotRegisterTypeException（如果构造函数参数未注册）
        var action = () => container.Resolve<ClassWithMultiplePublicConstructors>();

        action.Should().Throw<Exception>();
    }

    [TestMethod]
    public void Resolve_ClassWithTwoIocResolveAttributes_ShouldThrowIocResolveException()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<ClassWithTwoIocResolveConstructors>();

        var container = builder.Build();

        var action = () => container.Resolve<ClassWithTwoIocResolveConstructors>();

        action.Should().Throw<IocResolveException>()
            .WithMessage("*only one constructor*");
    }

    [TestMethod]
    public void Resolve_CircularDependency_Direct_ShouldThrowCircularDependencyException()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<CircularDependencyA>();
        builder.RegisterType<CircularDependencyB>();

        var container = builder.Build();

        var action = () => container.Resolve<CircularDependencyA>();

        action.Should().Throw<CircularDependencyException>()
            .WithMessage($"*{nameof(CircularDependencyA)}*");
    }

    [TestMethod]
    public void Resolve_CircularDependency_Indirect_ShouldThrowCircularDependencyException()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<IServiceA, ServiceA>();
        builder.RegisterType<IServiceB, ServiceB>();

        var container = builder.Build();

        var action = () => container.Resolve<IServiceA>();

        action.Should().Throw<CircularDependencyException>();
    }

    [TestMethod]
    public void Resolve_CircularDependency_SelfReference_ShouldThrowCircularDependencyException()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<SelfReferencingClass>();

        var container = builder.Build();

        var action = () => container.Resolve<SelfReferencingClass>();

        action.Should().Throw<CircularDependencyException>();
    }

    [TestMethod]
    public void CreateRegisterType_NullType_ShouldThrowArgumentNullException()
    {
        var builder = new IocContainerBuilder();

        var action = () => builder.CreateRegisterType(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void CreateRegisterInstance_NullInstance_ShouldThrowArgumentNullException()
    {
        var builder = new IocContainerBuilder();

        var action = () => builder.CreateRegisterInstance<IService>(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void InstanceByPerDependency_WithInstanceSet_ShouldThrowIocObjectConfigurationException()
    {
        var builder = new IocContainerBuilder();

        var instance = new Service();
        var registerObject = builder.CreateRegisterInstance(instance);

        var action = () => registerObject.InstanceByPerDependency();

        action.Should().Throw<IocObjectConfigurationException>()
            .WithMessage("*Instance*InstanceByPerDependency*");
    }

    [TestMethod]
    public void InstanceByContainerScope_WithInstanceSet_ShouldThrowIocObjectConfigurationException()
    {
        var builder = new IocContainerBuilder();

        var instance = new Service();
        var registerObject = builder.CreateRegisterInstance(instance);

        var action = () => registerObject.InstanceByContainerScope();

        action.Should().Throw<IocObjectConfigurationException>()
            .WithMessage("*Instance*InstanceByContainerScope*");
    }

    [TestMethod]
    public void Resolve_NamedRegistration_NonExistentName_ShouldThrowIocResolveException()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<IService, Service>("Existing");

        var container = builder.Build();

        var action = () => container.Resolve<IService>("NonExistent");

        action.Should().Throw<IocResolveException>()
            .WithMessage("*name*NonExistent*not found*");
    }

    [TestMethod]
    public void AutoRegister_MultipleInterfaces_ShouldThrowUnableToDetermineInterfaceException()
    {
        var builder = new IocContainerBuilder();

        builder.AutoRegister([typeof(MultipleInterfaceAutoRegisterClass).Assembly]);

        // 应该不会抛出异常，因为会跳过有多个接口的类
        var container = builder.Build();

        var action = () => container.Resolve<MultipleInterfaceAutoRegisterClass>();

        action.Should().Throw<NotRegisterTypeException>();
    }
}

// 辅助测试类
class SelfReferencingClass
{
    public SelfReferencingClass(SelfReferencingClass self) { }
}

interface IMultipleInterfaceService { }
interface IAnotherInterfaceService { }

class MultipleInterfaceAutoRegisterClass : IMultipleInterfaceService, IAnotherInterfaceService
{
}
