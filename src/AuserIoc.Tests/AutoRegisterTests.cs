using AuserIoc.Common.Attributes;
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
    }
}