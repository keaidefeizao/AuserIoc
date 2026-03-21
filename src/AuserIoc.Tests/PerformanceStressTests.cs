using System.Collections.Concurrent;
using System.Diagnostics;
using FluentAssertions;

namespace AuserIoc.Tests;

[TestClass]
public class PerformanceStressTests
{
    #region Test classes

    public interface IRepository<T>
    {
        T Get();
    }

    public class Repository<T> : IRepository<T>
    {
        public T Get() => default!;
    }

    public interface IService
    {
        int Id { get; }
        string Name { get; }
    }

    public class Service : IService
    {
        public Service()
        {
            Id = 1;
            Name = "Service";
        }

        public int Id { get; }
        public string Name { get; }
    }

    public interface IDependency
    {
        string Value { get; }
    }

    public class Dependency : IDependency
    {
        public string Value => "DependencyValue";
    }

    public class ServiceWithDependency : IService
    {
        private readonly IDependency _dependency;

        public ServiceWithDependency(IDependency dependency)
        {
            _dependency = dependency;
            Id = 2;
            Name = "ServiceWithDependency";
        }

        public int Id { get; }
        public string Name { get; }
    }

    public class ServiceWithMultipleDependencies : IService
    {
        private readonly IDependency _dependency1;
        private readonly IDependency _dependency2;
        private readonly IDependency _dependency3;

        public ServiceWithMultipleDependencies(
            IDependency dependency1,
            IDependency dependency2,
            IDependency dependency3)
        {
            _dependency1 = dependency1;
            _dependency2 = dependency2;
            _dependency3 = dependency3;
            Id = 3;
            Name = "ServiceWithMultipleDependencies";
        }

        public int Id { get; }
        public string Name { get; }
    }

    #endregion

    #region Helper methods

    private static IocContainerBuilder CreateBuilderWithServices()
    {
        var builder = new IocContainerBuilder();
        builder.RegisterSingleton<IDependency, Dependency>();
        builder.RegisterSingleton<IService, Service>();
        return builder;
    }

    private static IocContainerBuilder CreateBuilderWithMultipleServices()
    {
        var builder = new IocContainerBuilder();
        builder.RegisterSingleton<IDependency, Dependency>();
        builder.RegisterScoped<IService, ServiceWithDependency>();
        builder.RegisterType<IService, ServiceWithMultipleDependencies>("MultiDep");
        return builder;
    }

    #endregion

    #region 单例性能测试

    [TestMethod]
    public void Performance_Singleton_10000Resolves_ShouldCompleteWithinTime()
    {
        var builder = CreateBuilderWithServices();
        var container = builder.Build();

        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 10000; i++)
        {
            var service = container.Resolve<IService>();
            service.Should().NotBeNull();
        }

        stopwatch.Stop();

        // 10000 次单例解析应在 100ms 内完成
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100,
            $"10000 次单例解析耗时 {stopwatch.ElapsedMilliseconds}ms，超过 100ms 阈值");
    }

    [TestMethod]
    public void Performance_Singleton_100000Resolves_ShouldCompleteWithinTime()
    {
        var builder = CreateBuilderWithServices();
        var container = builder.Build();

        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 100000; i++)
        {
            var service = container.Resolve<IService>();
            service.Should().NotBeNull();
        }

        stopwatch.Stop();

        // 100000 次单例解析应在 500ms 内完成
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500,
            $"100000 次单例解析耗时 {stopwatch.ElapsedMilliseconds}ms，超过 500ms 阈值");
    }

    #endregion

    #region PerDependency 性能测试

    [TestMethod]
    public void Performance_PerDependency_10000Resolves_ShouldCompleteWithinTime()
    {
        var builder = new IocContainerBuilder();
        builder.RegisterSingleton<IDependency, Dependency>();
        builder.RegisterType<IService, ServiceWithDependency>();
        var container = builder.Build();

        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 10000; i++)
        {
            var service = container.Resolve<IService>();
            service.Should().NotBeNull();
        }

        stopwatch.Stop();

        // 10000 次 PerDependency 解析应在 500ms 内完成
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500,
            $"10000 次 PerDependency 解析耗时 {stopwatch.ElapsedMilliseconds}ms，超过 500ms 阈值");
    }

    [TestMethod]
    public void Performance_PerDependency_50000Resolves_ShouldCompleteWithinTime()
    {
        var builder = new IocContainerBuilder();
        builder.RegisterSingleton<IDependency, Dependency>();
        builder.RegisterType<IService, Service>();
        var container = builder.Build();

        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 50000; i++)
        {
            var service = container.Resolve<IService>();
            service.Should().NotBeNull();
        }

        stopwatch.Stop();

        // 50000 次简单 PerDependency 解析应在 300ms 内完成
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(300,
            $"50000 次简单 PerDependency 解析耗时 {stopwatch.ElapsedMilliseconds}ms，超过 300ms 阈值");
    }

    #endregion

    #region 并发性能测试

    [TestMethod]
    public void Performance_Concurrent_100Threads_1000ResolvesEach_ShouldCompleteWithinTime()
    {
        var builder = CreateBuilderWithServices();
        var container = builder.Build();

        var stopwatch = Stopwatch.StartNew();
        var tasks = new Task[100];

        for (int t = 0; t < 100; t++)
        {
            tasks[t] = Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    var service = container.Resolve<IService>();
                    service.Should().NotBeNull();
                }
            });
        }

        Task.WaitAll(tasks);
        stopwatch.Stop();

        // 100 个线程各 1000 次解析（共 100000 次）应在 1000ms 内完成
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000,
            $"100 线程 x1000 次解析耗时 {stopwatch.ElapsedMilliseconds}ms，超过 1000ms 阈值");
    }

    [TestMethod]
    public void Performance_Concurrent_50Threads_ParSingletonResolves_ShouldBeThreadSafe()
    {
        var builder = new IocContainerBuilder();
        builder.RegisterSingleton<IService, Service>();
        var container = builder.Build();

        var resolvedServices = new ConcurrentBag<IService>();
        var tasks = new Task[50];

        for (int t = 0; t < 50; t++)
        {
            tasks[t] = Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    resolvedServices.Add(container.Resolve<IService>());
                }
            });
        }

        Task.WaitAll(tasks);

        // 单例模式应该返回同一个实例
        resolvedServices.Distinct().Count().Should().Be(1,
            "单例模式在并发解析时应该返回同一个实例");
    }

    [TestMethod]
    public void Performance_Concurrent_50Threads_ParScopedResolves_ShouldBeThreadSafe()
    {
        var builder = new IocContainerBuilder();
        builder.RegisterScoped<IService, Service>();
        var container = builder.Build();

        var resolvedServices = new ConcurrentBag<IService>();
        var tasks = new Task[50];

        for (int t = 0; t < 50; t++)
        {
            tasks[t] = Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    resolvedServices.Add(container.Resolve<IService>());
                }
            });
        }

        Task.WaitAll(tasks);

        // Scoped 模式在同一个容器内应该返回同一个实例
        resolvedServices.Distinct().Count().Should().Be(1,
            "Scoped 模式在同一个容器内解析应该返回同一个实例");
    }

    #endregion

    #region 容器作用域性能测试

    [TestMethod]
    public void Performance_ContainerScope_1000Scopes_ShouldCompleteWithinTime()
    {
        var builder = CreateBuilderWithServices();
        var container = builder.Build();

        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 1000; i++)
        {
            using var scope = container.BeginContainerScope();
            var service = scope.Resolve<IService>();
            service.Should().NotBeNull();
        }

        stopwatch.Stop();

        // 1000 个作用域创建和解析应在 200ms 内完成
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200,
            $"1000 个作用域创建耗时 {stopwatch.ElapsedMilliseconds}ms，超过 200ms 阈值");
    }

    [TestMethod]
    public void Performance_ContainerScope_100Scopes_100ResolvesEach_ShouldCompleteWithinTime()
    {
        var builder = new IocContainerBuilder();
        builder.RegisterScoped<IService, Service>();
        var container = builder.Build();

        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 100; i++)
        {
            using var scope = container.BeginContainerScope();
            for (int j = 0; j < 100; j++)
            {
                var service = scope.Resolve<IService>();
                service.Should().NotBeNull();
            }
        }

        stopwatch.Stop();

        // 100 个作用域各 100 次解析应在 500ms 内完成
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500,
            $"100 个作用域 x100 次解析耗时 {stopwatch.ElapsedMilliseconds}ms，超过 500ms 阈值");
    }

    #endregion

    #region 泛型性能测试

    [TestMethod]
    public void Performance_GenericType_1000DifferentTypes_ShouldCompleteWithinTime()
    {
        var builder = new IocContainerBuilder();

        // 注册开放泛型
        builder.RegisterSingleton(typeof(IRepository<>), typeof(Repository<>));

        var container = builder.Build();

        var stopwatch = Stopwatch.StartNew();

        // 解析 1000 个不同的泛型类型
        for (int i = 0; i < 1000; i++)
        {
            var genericType = typeof(IRepository<>).MakeGenericType(GetTestType(i));
            var service = container.Resolve(genericType);
            service.Should().NotBeNull();
        }

        stopwatch.Stop();

        // 1000 个泛型类型解析应在 300ms 内完成
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(300,
            $"1000 个泛型类型解析耗时 {stopwatch.ElapsedMilliseconds}ms，超过 300ms 阈值");
    }

    private static Type GetTestType(int index)
    {
        int mod = index % 5;
        if (mod == 0) return typeof(string);
        if (mod == 1) return typeof(int);
        if (mod == 2) return typeof(double);
        if (mod == 3) return typeof(Guid);
        return typeof(DateTime);
    }

    #endregion

    #region 工厂方法性能测试

    [TestMethod]
    public void Performance_FactoryMethod_10000Resolves_ShouldCompleteWithinTime()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IService>(_ => new Service());

        var container = builder.Build();

        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 10000; i++)
        {
            var service = container.Resolve<IService>();
            service.Should().NotBeNull();
        }

        stopwatch.Stop();

        // 10000 次工厂方法解析应在 150ms 内完成
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(150,
            $"10000 次工厂方法解析耗时 {stopwatch.ElapsedMilliseconds}ms，超过 150ms 阈值");
    }

    [TestMethod]
    public void Performance_FactoryMethod_WithDependencies_5000Resolves_ShouldCompleteWithinTime()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IDependency, Dependency>();
        builder.RegisterSingleton<IService>(container =>
            new ServiceWithDependency(container.Resolve<IDependency>()));

        var container = builder.Build();

        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 5000; i++)
        {
            var service = container.Resolve<IService>();
            service.Should().NotBeNull();
        }

        stopwatch.Stop();

        // 5000 次带依赖的工厂方法解析应在 200ms 内完成
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200,
            $"5000 次带依赖的工厂方法解析耗时 {stopwatch.ElapsedMilliseconds}ms，超过 200ms 阈值");
    }

    #endregion

    #region 内存压力测试

    [TestMethod]
    public void Performance_MemoryPressure_10000PerDependencyInstances_ShouldNotLeak()
    {
        var builder = new IocContainerBuilder();
        builder.RegisterType<IService, Service>();
        var container = builder.Build();

        var initialMemory = GC.GetTotalMemory(true);

        // 创建 10000 个实例
        var services = new List<IService>();
        for (int i = 0; i < 10000; i++)
        {
            services.Add(container.Resolve<IService>());
        }

        var afterResolveMemory = GC.GetTotalMemory(true);

        // 释放引用
        services.Clear();

        // 强制 GC
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var afterGcMemory = GC.GetTotalMemory(true);

        // 内存增长应该合理（每实例约 1KB 以内）
        var memoryGrowth = afterResolveMemory - initialMemory;
        memoryGrowth.Should().BeLessThan(50 * 1024 * 1024, // 50MB 上限
            $"内存增长 {memoryGrowth / 1024 / 1024:F2}MB 超过预期");

        // GC 后内存应该接近初始值
        var memoryAfterGc = afterGcMemory - initialMemory;
        memoryAfterGc.Should().BeLessThan(10 * 1024 * 1024, // 10MB 上限
            $"GC 后内存增长 {memoryAfterGc / 1024 / 1024:F2}MB，可能存在内存泄漏");
    }

    #endregion

    #region 综合压力测试

    [TestMethod]
    public void Performance_Comprehensive_MixedOperations_ShouldCompleteWithinTime()
    {
        var builder = new IocContainerBuilder();

        // 注册多种服务
        builder.RegisterSingleton<IDependency, Dependency>();
        builder.RegisterScoped<IService, ServiceWithDependency>();
        builder.RegisterType<IService, ServiceWithMultipleDependencies>("Multi");
        builder.RegisterSingleton<IService, Service>("Simple");

        var container = builder.Build();

        var stopwatch = Stopwatch.StartNew();

        // 混合操作
        for (int i = 0; i < 1000; i++)
        {
            // 单例解析
            var singleton = container.Resolve<IService>("Simple");

            // 作用域解析
            using var scope = container.BeginContainerScope();
            var scoped = scope.Resolve<IService>();

            // PerDependency 解析
            var transient = container.Resolve<IService>("Multi");

            // 命名解析
            var named = container.Resolve<IService>("Multi");
        }

        stopwatch.Stop();

        // 综合操作应在 1000ms 内完成
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000,
            $"综合压力测试耗时 {stopwatch.ElapsedMilliseconds}ms，超过 1000ms 阈值");
    }

    [TestMethod]
    public void Performance_Stress_100000MixedResolves_ShouldCompleteWithinTime()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IDependency, Dependency>();
        builder.RegisterSingleton<IService, Service>();

        var container = builder.Build();

        var stopwatch = Stopwatch.StartNew();

        // 100000 次简单解析
        for (int i = 0; i < 100000; i++)
        {
            _ = container.Resolve<IService>();
        }

        stopwatch.Stop();

        Console.WriteLine($"100000 次解析耗时：{stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"平均每次解析：{stopwatch.ElapsedMilliseconds / 100000.0 * 1000:F3}μs");

        // 100000 次解析应在 800ms 内完成
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(800,
            $"100000 次解析耗时 {stopwatch.ElapsedMilliseconds}ms，超过 800ms 阈值");
    }

    #endregion
}
