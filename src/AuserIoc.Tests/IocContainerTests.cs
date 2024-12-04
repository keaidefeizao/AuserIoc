using AuserIoc.Exceptions;
using AuserIoc.Tests.TestObjects;
using FluentAssertions;

namespace AuserIoc.Tests;

[TestClass]
public class IocContainerTests
{
    #region class

    internal class TestService(IRepository<UserEntity> userRepo,
                               IRepository<DocEntity> docRepo,
                               IRepository<object> objectRepo,
                               IRepository<SqliteContextTag, UserEntity> sqliteUserRepo,
                               IRepository<SqliteContextTag, DocEntity> sqliteDocRepo,
                               IRepository<SqliteContextTag, object> sqliteObjectRepo)
    {
        internal void Test()
        {
            userRepo.Add(new UserEntity());
            docRepo.Add(new DocEntity());
            objectRepo.Add(new object());
        }

        internal void Test2()
        {
            sqliteUserRepo.Add(new UserEntity());
            sqliteDocRepo.Add(new DocEntity());
            sqliteObjectRepo.Add(new object());
        }
    }

    internal class CircularDependencyService(CircularDependencyService circularDependencyService)
    {
        public CircularDependencyService Service { get; } = circularDependencyService;
    }

    internal class AService(BService bService)
    {
        public BService BService { get; } = bService;
    }
    internal class BService(AService aService)
    {
        public AService AService { get; } = aService;
    }
    internal class CService(DService dService)
    {
        public DService DService { get; } = dService;
    }
    internal class DService(EService eService)
    {
        public EService EService { get; } = eService;
    }
    internal class EService(CService cService)
    {
        public CService CService { get; } = cService;
    }

    #endregion class

    /// <summary>
    /// 未确定泛型参数解析测试
    /// </summary>
    [TestMethod]
    public void GenericArgumentsTest()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<TestService>();
        builder.RegisterScoped<AppDbContext>();
        builder.RegisterScoped<SqliteAppDbContext>();
        builder.RegisterType(typeof(IRepository<>), typeof(Repository<>));
        builder.RegisterType(typeof(IRepository<,>), typeof(Repository<,>));

        var container = builder.Build();

        var testService = container.Resolve<TestService>();

        testService.Test();
        testService.Test2();

        _ = container.Resolve<TestService>();
    }

    /// <summary>
    /// 循环依赖测试
    /// </summary>
    [TestMethod]
    public void CircularDependencyTest()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<CircularDependencyService>();
        builder.RegisterType<AService>();
        builder.RegisterType<BService>();
        builder.RegisterType<CService>();
        builder.RegisterType<DService>();
        builder.RegisterType<EService>();

        var container = builder.Build();

        var action = () =>
        {
            _ = container.Resolve<CircularDependencyService>();
        };

        action.Should().Throw<CircularDependencyException>();

        action = () =>
        {
            _ = container.Resolve<AService>();
        };

        action.Should().Throw<CircularDependencyException>();

        action = () =>
        {
            _ = container.Resolve<BService>();
        };

        action.Should().Throw<CircularDependencyException>();

        action = () =>
        {
            _ = container.Resolve<CService>();
        };

        action.Should().Throw<CircularDependencyException>();
    }

    /// <summary>
    /// 解析错误测试
    /// </summary>
    [TestMethod]
    public void ResolveFaultTest()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterType<TestService>();

        var container = builder.Build();

        var action = () =>
        {
            _ = container.Resolve<TestService>();
        };

        action.Should().Throw<NotRegisterTypeException>();
    }

    /// <summary>
    /// 工程方法的解析测试
    /// </summary>
    [TestMethod]
    public void FactoryMethodTest()
    {
        var builder = new IocContainerBuilder();

        builder
            .RegisterType<TestService>((IRepository<UserEntity> userRepo,
                               IRepository<DocEntity> docRepo,
                               IRepository<object> objectRepo,
                               IRepository<SqliteContextTag, UserEntity> sqliteUserRepo,
                               IRepository<SqliteContextTag, DocEntity> sqliteDocRepo,
                               IRepository<SqliteContextTag, object> sqliteObjectRepo) =>
            {
                return new TestService(userRepo, docRepo, objectRepo, sqliteUserRepo, sqliteDocRepo, sqliteObjectRepo);
            });

        builder.RegisterScoped<AppDbContext>();
        builder.RegisterScoped<SqliteAppDbContext>();
        builder.RegisterType(typeof(IRepository<>), typeof(Repository<>));
        builder.RegisterType(typeof(IRepository<,>), typeof(Repository<,>));

        var container = builder.Build();

        var testService = container.Resolve<TestService>();

        testService.Test();
        testService.Test2();
    }

    /// <summary>
    /// 注册实例测试
    /// </summary>
    [TestMethod]
    public void RegisterInstanceTest()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterInstance(new AppDbContext());
        builder.RegisterInstance(new SqliteAppDbContext());

        var container = builder.Build();

        var appDbContext1 = container.Resolve<AppDbContext>();
        var sqliteAppDbContext1 = container.Resolve<SqliteAppDbContext>();

        var appDbContext2 = container.Resolve<AppDbContext>();
        var sqliteAppDbContext2 = container.Resolve<SqliteAppDbContext>();

        (appDbContext1 == appDbContext2).Should().BeTrue();
        (sqliteAppDbContext1 == sqliteAppDbContext2).Should().BeTrue();

        var container2 = container.BeginContainerScope();

        var appDbContext3 = container2.Resolve<AppDbContext>();
        var sqliteAppDbContext3 = container2.Resolve<SqliteAppDbContext>();

        (appDbContext1 == appDbContext3).Should().BeTrue();
        (sqliteAppDbContext1 == sqliteAppDbContext3).Should().BeTrue();
    }
}
