using AuserIoc.Tests.TestObjects;
using FluentAssertions;

namespace AuserIoc.Tests;

[TestClass]
public class SingletonTests
{
    [TestMethod]
    public void SingletonTest()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IUniverse, Universe>();
        builder.RegisterSingleton<IEarth, Earth>();
        builder.RegisterSingleton<IMoon, Moon>();

        var container = builder.Build();

        var universe1 = container.Resolve<IUniverse>();
        var universe2 = container.Resolve<IUniverse>();

        var earth = container.Resolve<IEarth>();
        var moon = container.Resolve<IMoon>();

        (universe1 == universe2).Should().BeTrue();
        (universe1.Earth == earth).Should().BeTrue();
        (universe2.Moon == moon).Should().BeTrue();
    }

    [TestMethod]
    public void ThreadSingletonTest()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterSingleton<IUniverse, Universe>();
        builder.RegisterSingleton<IEarth, Earth>();
        builder.RegisterSingleton<IMoon, Moon>();

        var container = builder.Build();

        var universeTask1 = Task.Run(() => container.Resolve<IUniverse>());
        var universeTask2 = Task.Run(() => container.Resolve<IUniverse>());

        var earthTask = Task.Run(() => container.Resolve<IEarth>());
        var moonTask = Task.Run(() => container.Resolve<IMoon>());

        Task.WhenAll(universeTask1, universeTask2, earthTask, moonTask);

        var universe1 = universeTask1.Result;
        var universe2 = universeTask2.Result;

        var earth = earthTask.Result;
        var moon = moonTask.Result;

        (universe1 == universe2).Should().BeTrue();
        (universe1.Earth == earth).Should().BeTrue();
        (universe2.Moon == moon).Should().BeTrue();
    }

    /// <summary>
    /// 线程安全单例测试
    /// </summary>
    [TestMethod]
    public void ThreadSafeSingletonTest()
    {
        var builder = new IocContainerBuilder();
        var builder2 = new IocContainerBuilder();//新的构建器代表一个全新构建容器的环境，构建器之间不共享注册类型。

        builder.RegisterSingleton<IUniverse, Universe>();
        builder.RegisterSingleton<IEarth, Earth>();
        builder.RegisterSingleton<IMoon, Moon>();

        builder2.RegisterSingleton<IUniverse, Universe>();
        builder2.RegisterSingleton<IEarth, Earth>();
        builder2.RegisterSingleton<IMoon, Moon>();

        var container = builder.Build();
        var containerScope = container.BeginContainerScope();
        var container2 = builder2.Build();

        var tasks = new Task<IUniverse>[500];
        var task2 = new Task<IUniverse>[500];
        var task3 = new Task<IUniverse>[500];

        for (int i = 0; i < 500; i++)
        {
            tasks[i] = Task.Run(() => container.Resolve<IUniverse>());
            task2[i] = Task.Run(() => containerScope.Resolve<IUniverse>());
            task3[i] = Task.Run(() => container2.Resolve<IUniverse>());
        }

        Task<IUniverse>[] allTask = [.. tasks, .. task2, .. task3];

        Task.WhenAll(allTask);

        allTask.Select(t => t.Result).Distinct().Should().HaveCount(2);
        allTask.Select(t => t.Result.Earth).Distinct().Should().HaveCount(2);
        allTask.Select(t => t.Result.Moon).Distinct().Should().HaveCount(2);
    }
}
