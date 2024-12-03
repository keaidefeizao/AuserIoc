# AuserIoc

#### Description
Pure dotNET toy level container, available.

#### Software Architecture
Supports net4.6.2, net4.7.2, net4.8.1, net6, net8, and net9

#### Installation
    dotnet add package AuserIoc
    
#### Usage
``` csharp
public class AService(IBService bService)
{
    private readonly IBService _bService = bService;
    public bool CompleteInjection() => _bService != null;
}

public interface IBService { }

public class BService() : IBService { }

// 使用
var builder = new IocContainerBuilder();

builder.RegisterType<IBService>();
builder.RegisterSingleton<AService>();

var container = builder.Build();

var aService = container.Resolve<AService>();
```
