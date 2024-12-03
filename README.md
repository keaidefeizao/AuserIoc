# AuserIoc

#### 介绍
纯 dotNET 的 玩具级容器，能用。

#### 软件架构
支持net4.6.2、net4.7.2、net4.8.1、net6、net8、net9

#### 安装
    dotnet add package AuserIoc

#### 使用方法
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