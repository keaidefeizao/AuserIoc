using AuserIoc.Common.Attributes;
using System.Reflection;

namespace AuserIoc.Tests;

[TestClass]
public class AutoRegisterTests
{
    public interface A { }

    public interface B { }

    [Singleton]
    class AutoRegisterInterfaceClass : A
    {

    }

    class TestClass
    {
        string GetName()
        {
            return "name";
        }
    }

    [Singleton]
    class ImpTestClass : TestClass
    {
        string Name => "name";
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
