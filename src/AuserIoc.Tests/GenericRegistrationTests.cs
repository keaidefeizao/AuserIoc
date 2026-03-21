using AuserIoc.Tests.TestObjects;
using FluentAssertions;

namespace AuserIoc.Tests;

[TestClass]
public class GenericRegistrationTests
{
    #region 测试类

    interface IGenericService<T>
    {
        T Value { get; }
    }

    class GenericService<T> : IGenericService<T>
    {
        public T Value { get; set; } = default!;
    }

    interface IMultiGenericService<T1, T2>
    {
        T1 Value1 { get; }
        T2 Value2 { get; }
    }

    class MultiGenericService<T1, T2> : IMultiGenericService<T1, T2>
    {
        public T1 Value1 { get; set; } = default!;
        public T2 Value2 { get; set; } = default!;
    }

    class StringService : IGenericService<string>
    {
        public string Value => "StringValue";
    }

    class IntService : IGenericService<int>
    {
        public int Value => 42;
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
    public void RegisterType_OpenGenericType_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType(typeof(IGenericService<>), typeof(GenericService<>));

        var container = builder.Build();

        var stringService = container.Resolve<IGenericService<string>>();
        var intService = container.Resolve<IGenericService<int>>();

        stringService.Should().NotBeNull();
        intService.Should().NotBeNull();
        stringService.Should().BeOfType<GenericService<string>>();
        intService.Should().BeOfType<GenericService<int>>();
    }

    [TestMethod]
    public void RegisterType_OpenGenericType_WithMultipleArguments_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType(typeof(IMultiGenericService<,>), typeof(MultiGenericService<,>));

        var container = builder.Build();

        var service1 = container.Resolve<IMultiGenericService<string, int>>();
        var service2 = container.Resolve<IMultiGenericService<double, string>>();

        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().BeOfType<MultiGenericService<string, int>>();
        service2.Should().BeOfType<MultiGenericService<double, string>>();
    }

    [TestMethod]
    public void RegisterType_ClosedGenericType_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<StringService>();

        var container = builder.Build();

        var service = container.Resolve<StringService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<StringService>();
        service.Value.Should().Be("StringValue");
    }

    [TestMethod]
    public void RegisterType_OpenGenericType_AsSingleton_ShouldReturnSameInstance()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton(typeof(IGenericService<>), typeof(GenericService<>));

        var container = builder.Build();

        var service1 = container.Resolve<IGenericService<string>>();
        var service2 = container.Resolve<IGenericService<string>>();

        service1.Should().BeSameAs(service2);
    }

    [TestMethod]
    public void RegisterType_OpenGenericType_AsScoped_ShouldReturnSameInstanceInScope()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterScoped(typeof(IGenericService<>), typeof(GenericService<>));

        var container = builder.Build();

        var service1 = container.Resolve<IGenericService<string>>();
        var service2 = container.Resolve<IGenericService<string>>();

        service1.Should().BeSameAs(service2);
    }

    [TestMethod]
    public void RegisterType_OpenGenericType_AsScoped_DifferentInstancesAcrossScopes()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterScoped(typeof(IGenericService<>), typeof(GenericService<>));

        var container = builder.Build();
        var scope = container.BeginContainerScope();

        var service1 = container.Resolve<IGenericService<string>>();
        var service2 = scope.Resolve<IGenericService<string>>();

        service1.Should().NotBeSameAs(service2);
    }

    [TestMethod]
    public void RegisterType_OpenGenericType_WithDependency_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IService, Service>();
        builder.RegisterType(typeof(IGenericService<>), typeof(GenericService<>));

        var container = builder.Build();

        var genericService = container.Resolve<IGenericService<IService>>();

        genericService.Should().NotBeNull();
        genericService.Should().BeOfType<GenericService<IService>>();
    }

    [TestMethod]
    public void RegisterType_MultipleOpenGenericTypes_ShouldResolveIndependently()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType(typeof(IGenericService<>), typeof(GenericService<>));
        builder.RegisterType(typeof(IMultiGenericService<,>), typeof(MultiGenericService<,>));

        var container = builder.Build();

        var genericService = container.Resolve<IGenericService<string>>();
        var multiService = container.Resolve<IMultiGenericService<string, int>>();

        genericService.Should().NotBeNull();
        multiService.Should().NotBeNull();
    }

    [TestMethod]
    public void RegisterType_OpenGenericType_WithName_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType(typeof(IGenericService<>), typeof(GenericService<>), "Named");

        var container = builder.Build();

        var service = container.Resolve<IGenericService<string>>("Named");

        service.Should().NotBeNull();
        service.Should().BeOfType<GenericService<string>>();
    }

    [TestMethod]
    public void RegisterType_ClosedGenericType_WithName_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<IGenericService<string>, StringService>("StringService");
        builder.RegisterType<IGenericService<int>, IntService>("IntService");

        var container = builder.Build();

        var stringService = container.Resolve<IGenericService<string>>("StringService");
        var intService = container.Resolve<IGenericService<int>>("IntService");

        stringService.Should().BeOfType<StringService>();
        intService.Should().BeOfType<IntService>();
        stringService.Value.Should().Be("StringValue");
        intService.Value.Should().Be(42);
    }

    [TestMethod]
    public void RegisterType_OpenGenericType_DifferentClosedTypes_ReturnDifferentInstances()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType(typeof(IGenericService<>), typeof(GenericService<>));

        var container = builder.Build();

        var stringService = container.Resolve<IGenericService<string>>();
        var intService = container.Resolve<IGenericService<int>>();

        stringService.Should().NotBeSameAs(intService);
    }

    [TestMethod]
    public void RegisterType_GenericTypeWithConstraint_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType(typeof(IRepository<>), typeof(Repository<>));
        builder.RegisterScoped<AppDbContext>();

        var container = builder.Build();

        var repo = container.Resolve<IRepository<UserEntity>>();

        repo.Should().NotBeNull();
        repo.Should().BeOfType<Repository<UserEntity>>();
    }

    [TestMethod]
    public void RegisterType_MultiLevelGenericType_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType(typeof(IRepository<,>), typeof(Repository<,>));
        builder.RegisterScoped<AppDbContext>();

        var container = builder.Build();

        var repo = container.Resolve<IRepository<AppDbContext, UserEntity>>();

        repo.Should().NotBeNull();
        repo.Should().BeOfType<Repository<AppDbContext, UserEntity>>();
    }
}
