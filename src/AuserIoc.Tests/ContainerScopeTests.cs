using AuserIoc.Tests.TestObjects;
using FluentAssertions;

namespace AuserIoc.Tests;

[TestClass]
public class ContainerScopeTests
{
    [TestMethod]
    public void ContainerScopeTest()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterScoped<IUniverse, Universe>();
        builder.RegisterScoped<IEarth, Earth>();
        builder.RegisterScoped<IMoon, Moon>();

        var container = builder.Build();

        var universe1 = container.Resolve<IUniverse>();
        var universe2 = container.Resolve<IUniverse>();

        var earth = container.Resolve<IEarth>();
        var moon = container.Resolve<IMoon>();

        (universe1 == universe2).Should().BeTrue();
        (universe1.Earth == earth).Should().BeTrue();
        (universe2.Moon == moon).Should().BeTrue();

        var newContainer = container.BeginContainerScope();

        var newUniverse1 = newContainer.Resolve<IUniverse>();
        var newUniverse2 = newContainer.Resolve<IUniverse>();

        (newUniverse1 == newUniverse2).Should().BeTrue();

        (universe1 == newUniverse1).Should().BeFalse();
        (universe2 == newUniverse2).Should().BeFalse();

        (universe1.Earth == newUniverse1.Earth).Should().BeFalse();
        (universe1.Moon == newUniverse1.Moon).Should().BeFalse();
    }

    [TestMethod]
    public void DisposeTest()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterScoped<IUniverse, Universe>();
        builder.RegisterScoped<IEarth, Earth>();
        builder.RegisterSingleton<IMoon, Moon>();

        var container = builder.Build();

        IUniverse universe1;
        IUniverse universe2;

        using (container)
        {
            universe1 = container.Resolve<IUniverse>();
        }

        universe2 = container.Resolve<IUniverse>();

        (universe1 == universe2).Should().BeFalse();

        (universe1.Moon == universe2.Moon).Should().BeTrue();
    }

    [TestMethod]
    public void Test()
    {
        var builder = new IocContainerBuilder();

        builder.RegisterScoped<IUniverse, Universe>();
        builder.RegisterSingleton<IEarth, Earth>();
        builder.RegisterSingleton<IMoon, Moon>();

        var container = builder.Build();

        var universe1 = container.Resolve<IUniverse>();
        var universe1Id = universe1.GetHashCode();

        universe1 = null;

        var universe2 = container.Resolve<IUniverse>();
        var universe2Id = universe2.GetHashCode();

        (universe1Id == universe2Id).Should().BeTrue();
    }
}
