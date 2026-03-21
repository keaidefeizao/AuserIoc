using FluentAssertions;

namespace AuserIoc.Tests;

[TestClass]
public class TypeRegistrationTests
{
    #region Test classes

    public interface IService
    {
        string Name { get; }
    }

    public class TestService : IService
    {
        public string Name => "TestService";
    }

    public interface IRepository<T>
    {
        string TypeName { get; }
    }

    public class Repository<T> : IRepository<T>
    {
        public string TypeName => typeof(T).Name;
    }

    #endregion

    #region RegisterType with Type parameters and name

    [TestMethod]
    public void RegisterType_TypeFromTypeToTypeWithName_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType(typeof(IService), typeof(TestService), "MyService");

        var container = builder.Build();

        var service = container.Resolve<IService>("MyService");

        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [TestMethod]
    public void RegisterType_TypeWithName_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType(typeof(TestService), "MyService");

        var container = builder.Build();

        var service = container.Resolve<TestService>("MyService");

        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    #endregion

    #region RegisterScoped with Type parameters and name

    [TestMethod]
    public void RegisterScoped_TypeFromTypeToTypeWithName_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterScoped(typeof(IService), typeof(TestService), "MyService");

        var container = builder.Build();

        var service1 = container.Resolve<IService>("MyService");
        var service2 = container.Resolve<IService>("MyService");

        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().BeSameAs(service2);
    }

    [TestMethod]
    public void RegisterScoped_TypeWithName_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterScoped(typeof(TestService), "MyService");

        var container = builder.Build();

        var service1 = container.Resolve<TestService>("MyService");
        var service2 = container.Resolve<TestService>("MyService");

        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().BeSameAs(service2);
    }

    #endregion

    #region RegisterSingleton with Type parameters and name

    [TestMethod]
    public void RegisterSingleton_TypeFromTypeToTypeWithName_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton(typeof(IService), typeof(TestService), "MyService");

        var container = builder.Build();

        var service1 = container.Resolve<IService>("MyService");
        var service2 = container.Resolve<IService>("MyService");

        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().BeSameAs(service2);

        var scopeContainer = container.BeginContainerScope();
        var service3 = scopeContainer.Resolve<IService>("MyService");

        service3.Should().BeSameAs(service1);
    }

    [TestMethod]
    public void RegisterSingleton_TypeWithName_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton(typeof(TestService), "MyService");

        var container = builder.Build();

        var service1 = container.Resolve<TestService>("MyService");
        var service2 = container.Resolve<TestService>("MyService");

        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().BeSameAs(service2);
    }

    #endregion

    #region RegisterType with open generic types and name

    [TestMethod]
    public void RegisterType_OpenGenericTypeWithName_ShouldResolve()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType(typeof(IRepository<>), typeof(Repository<>), "MyRepo");

        var container = builder.Build();

        var stringRepo = container.Resolve<IRepository<string>>("MyRepo");
        var intRepo = container.Resolve<IRepository<int>>("MyRepo");

        stringRepo.Should().NotBeNull();
        stringRepo.TypeName.Should().Be("String");

        intRepo.Should().NotBeNull();
        intRepo.TypeName.Should().Be("Int32");
    }

    #endregion
}
