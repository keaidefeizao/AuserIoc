

# AuserIoc

## 介绍

AuserIoc 是一个轻量级的依赖注入（IOC）容器，旨在为 .NET 应用程序提供高效、易用的对象管理和依赖关系解析功能。它采用模块化设计，支持多种对象生命周期管理模式，帮助开发者轻松实现依赖注入。

## 功能特色

- **多种注入方式**：支持构造函数注入和方法参数注入，配合 `[IocResolveAttribute]` 特性精准控制解析逻辑。
- **灵活的生命周期管理**：
  - **单例（Singleton）**：整个容器生命周期内共享一个实例
  - **瞬态（PerDependency）**：每次请求创建全新的实例
  - **容器范围（ContainerScope）**：在同一容器范围内共享实例
- **模块化设计**：通过构建器模式和丰富的扩展方法，灵活自定义容器行为
- **自动注册功能**：支持程序集扫描和特性标记（`[Singleton]`、`[PerDependency]`、`[ContainerScope]`）自动注册
- **完善的异常处理**：提供多种专用异常类型，便于调试和错误排查
- **循环依赖检测**：自动检测并抛出清晰的循环依赖异常

## 软件架构

### 支持的框架

- .NET Framework 4.6.2
- .NET Framework 4.7.2
- .NET Framework 4.8.1
- .NET 6.0
- .NET 8.0
- .NET 9.0

### 核心组件

| 组件 | 说明 |
|------|------|
| `IocContainerBuilder` | 容器构建器，用于配置和构建 IOC 容器 |
| `IocContainer` | IOC 容器实现，负责对象解析和生命周期管理 |
| `RegisterObject` | 注册对象封装，包含类型、实例、工厂方法等信息 |
| `IocContainerBuilderExtension` | 扩展方法集合，提供便捷的注册 API |

### 异常体系

| 异常类型 | 说明 |
|----------|------|
| `AuserIocException` | 基础异常类 |
| `NotRegisterTypeException` | 类型未注册异常 |
| `RegisteredTypeException` | 类型重复注册异常 |
| `CircularDependencyException` | 循环依赖异常 |
| `IocResolveException` | 解析异常 |
| `IocObjectConfigurationException` | 对象配置异常 |
| `AutoRegisterException` | 自动注册异常 |
| `UnableToDetermineInterfaceException` | 无法确定接口异常 |

## 安装

```bash
dotnet add package AuserIoc
```

## 使用方法

### 基本注册与解析

```csharp
// 创建容器构建器
var containerBuilder = new IocContainerBuilder();

// 注册瞬态服务
containerBuilder.RegisterType<IService, ServiceImplementation>();

// 构建容器
var container = containerBuilder.Build();

// 解析服务实例
var service = container.Resolve<IService>();
service.DoSomething();
```

### 使用扩展方法注册

```csharp
var containerBuilder = new IocContainerBuilder();

// 注册类型（瞬态）
containerBuilder.RegisterType<IService, ServiceImplementation>();

// 注册具体类型（瞬态）
containerBuilder.RegisterType<ServiceImplementation>();

// 注册单例
containerBuilder.RegisterSingleton<IService, ServiceImplementation>();

// 注册容器范围
containerBuilder.RegisterScoped<IService, ServiceImplementation>();

// 注册实例
containerBuilder.RegisterInstance<IService>(new ServiceImplementation());

// 使用工厂方法注册
containerBuilder.RegisterType<IService>(() => new ServiceImplementation());

containerBuilder.RegisterSingleton<IService>(container => new ServiceImplementation());

var container = containerBuilder.Build();
```

### 生命周期管理

```csharp
var containerBuilder = new IocContainerBuilder();

// 瞬态 - 每次解析创建新实例（默认）
containerBuilder.RegisterType<IService, ServiceImplementation>();

// 单例 - 容器内共享实例
containerBuilder.RegisterSingleton<IService, ServiceImplementation>();

// 容器范围 - 容器范围内共享实例
containerBuilder.RegisterScoped<IService, ServiceImplementation>();

var container = containerBuilder.Build();
```

### 容器范围

```csharp
var containerBuilder = new IocContainerBuilder();

containerBuilder.RegisterScoped<IService, ServiceImplementation>();

var container = containerBuilder.Build();

// 创建容器范围
using (var scope = container.BeginContainerScope())
{
    var service1 = scope.Resolve<IService>();
    var service2 = scope.Resolve<IService>();
    // service1 和 service2 是同一实例
}
```

### 自动注册

```csharp
var containerBuilder = new IocContainerBuilder();

Assembly[] assemblys = [Assembly.GetExecutingAssembly()];

// 自动注册程序集中的服务
containerBuilder.AutoRegister(assemblys);

// 支持特性标记自动识别生命周期
// [Singleton] - 单例
// [PerDependency] - 瞬态
// [ContainerScope] - 容器范围}

[Singleton]
public class AutoService
{
    public void DoSomething()
    {
        Console.WriteLine("AutoService is doing something.");
    }
}
```

### 构造函数参数注入

```csharp
// 自动解析构造函数参数
public class Service : IService
{
    public Service(IRepository<User> userRepo, IRepository<Order> orderRepo)
    {
        // 参数会自动从容器中解析
    }
}

// 使用 IocResolveAttribute 指定构造函数
public class Service : IService
{
    [IocResolveAttribute]
    public Service(IRepository<User> userRepo)
    {
    }
}
```

### 泛型支持

```csharp
var containerBuilder = new IocContainerBuilder();

containerBuilder.RegisterType<IRepository<User>, Repository<User>>();
// or 
containerBuilder.RegisterType(typeof(IRepository<>), typeof(Repository<>));

var container = containerBuilder.Build();

var repository = container.Resolve<IRepository<User>>();
```

## 参与贡献

1. Fork 本仓库
2. 新建 Feat_xxx 分支
3. 提交代码
4. 新建 Pull Request

## 许可证

本项目基于 MIT 许可证开源。