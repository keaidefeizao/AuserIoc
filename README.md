# AuserIoc

[![NuGet](https://img.shields.io/nuget/v/AuserIoc.svg)](https://www.nuget.org/packages/AuserIoc)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## 介绍

AuserIoc 是一个轻量级的依赖注入（IOC）容器，旨在为 .NET 应用程序提供高效、易用的对象管理和依赖关系解析功能。它采用模块化设计，支持多种对象生命周期管理模式，帮助开发者轻松实现依赖注入。

## 功能特色

- **多种注入方式**：支持构造函数注入和方法参数注入，配合 `[IocResolve]` 特性精准控制解析逻辑
- **灵活的生命周期管理**：
  - **单例（Singleton）**：整个应用程序生命周期内共享一个实例
  - **瞬态（PerDependency）**：每次请求创建全新的实例
  - **容器范围（ContainerScope）**：在同一容器范围内共享实例
- **自动注册功能**：支持程序集扫描和特性标记（`[Singleton]`、`[PerDependency]`、`[ContainerScope]`）自动注册
- **泛型类型支持**：支持开放泛型和闭合泛型类型的注册与解析
- **命名注册**：支持同一接口多实现按名称区分
- **工厂方法注册**：支持自定义工厂方法创建实例
- **循环依赖检测**：运行时自动检测并抛出循环依赖异常
- **完善的异常处理**：提供多种专用异常类型，便于调试和错误排查
- **线程安全**：所有解析操作都是线程安全的，支持高并发场景

## 支持的框架

- .NET Framework 4.6.2 / 4.7.2 / 4.8.1
- .NET 6.0
- .NET 8.0
- .NET 9.0

## 安装

```bash
dotnet add package AuserIoc
```

## 快速开始

### 基本注册与解析

```csharp
using AuserIoc;

// 定义接口
public interface IMessageService
{
    void SendMessage(string message);
}

// 实现接口
public class MessageService : IMessageService
{
    public void SendMessage(string message)
    {
        Console.WriteLine($"Message: {message}");
    }
}

// 配置容器
var builder = new IocContainerBuilder();
builder.RegisterType<IMessageService, MessageService>();

// 构建容器
var container = builder.Build();

// 解析服务
var service = container.Resolve<IMessageService>();
service.SendMessage("Hello, AuserIoc!");
```

## 详细使用指南

### 1. 生命周期管理

#### 瞬态（PerDependency）- 每次解析创建新实例

```csharp
var builder = new IocContainerBuilder();

// 注册瞬态服务
builder.RegisterType<IMessageService, MessageService>();

var container = builder.Build();

var service1 = container.Resolve<IMessageService>();
var service2 = container.Resolve<IMessageService>();

// service1 和 service2 是不同的实例
Console.WriteLine(ReferenceEquals(service1, service2)); // False
```

#### 单例（Singleton）- 整个生命周期共享一个实例

```csharp
var builder = new IocContainerBuilder();

// 注册单例服务
builder.RegisterSingleton<IMessageService, MessageService>();

var container = builder.Build();

var service1 = container.Resolve<IMessageService>();
var service2 = container.Resolve<IMessageService>();

// service1 和 service2 是同一个实例
Console.WriteLine(ReferenceEquals(service1, service2)); // True
```

#### 容器范围（ContainerScope）- 容器范围内共享实例

```csharp
var builder = new IocContainerBuilder();

// 注册容器范围服务
builder.RegisterScoped<IMessageService, MessageService>();

var container = builder.Build();

// 在同一容器内解析
var service1 = container.Resolve<IMessageService>();
var service2 = container.Resolve<IMessageService>();
Console.WriteLine(ReferenceEquals(service1, service2)); // True

// 创建新的容器范围
// 注意：使用 using 语句确保作用域结束时释放资源
using (var scope = container.BeginContainerScope())
{
    var service3 = scope.Resolve<IMessageService>();
    // 不同范围内的实例不同
    Console.WriteLine(ReferenceEquals(service1, service3)); // False
}
// scope.Dispose() 在此自动调用
```

**注意**：`BeginContainerScope()` 返回的对象实现了 `IDisposable` 接口。当使用容器范围时，建议使用 `using` 语句包裹，确保作用域结束时正确释放资源。

### 2. 命名注册

当同一接口有多个实现时，可以使用命名注册来区分：

```csharp
public interface ILogger
{
    void Log(string message);
}

public class FileLogger : ILogger { }
public class DatabaseLogger : ILogger { }
public class ConsoleLogger : ILogger { }

var builder = new IocContainerBuilder();

// 使用名称注册多个实现
builder.RegisterType<ILogger, FileLogger>("File");
builder.RegisterType<ILogger, DatabaseLogger>("Database");
builder.RegisterType<ILogger, ConsoleLogger>("Console");

var container = builder.Build();

// 通过名称解析特定实现
var fileLogger = container.Resolve<ILogger>("File");
var dbLogger = container.Resolve<ILogger>("Database");
```

### 3. 工厂方法注册

#### 无参数工厂方法

```csharp
var builder = new IocContainerBuilder();

builder.RegisterType<IMessageService>(() => new MessageService());

var container = builder.Build();
var service = container.Resolve<IMessageService>();
```

#### 带容器参数的工厂方法

```csharp
var builder = new IocContainerBuilder();

builder.RegisterSingleton<IMessageService>(container =>
{
    // 可以从容器中解析其他依赖
    var config = container.Resolve<IConfiguration>();
    return new MessageService(config);
});
```

#### 使用 Delegate 工厂方法

```csharp
var builder = new IocContainerBuilder();

builder.RegisterType<IMessageService>((IMessageService service) =>
{
    return new MessageService();
});
```

### 4. 泛型类型注册

```csharp
public interface IRepository<T>
{
    T Get(int id);
    void Save(T entity);
}

public class Repository<T> : IRepository<T>
{
    public T Get(int id) => default(T);
    public void Save(T entity) { }
}

public class User { public int Id { get; set; } }
public class Order { public int Id { get; set; } }

var builder = new IocContainerBuilder();

// 注册开放泛型
builder.RegisterType(typeof(IRepository<>), typeof(Repository<>));

var container = builder.Build();

// 解析闭合泛型
var userRepository = container.Resolve<IRepository<User>>();
var orderRepository = container.Resolve<IRepository<Order>>();
```

### 5. 构造函数参数注入

AuserIoc 支持自动解析构造函数参数：

```csharp
public interface IUserService
{
    void ProcessUser(int userId);
}

public interface IOrderService
{
    void ProcessOrder(int orderId);
}

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;

    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public void ProcessUser(int userId) { }
}

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUserService _userService;

    public OrderService(IRepository<Order> orderRepository, IUserService userService)
    {
        _orderRepository = orderRepository;
        _userService = userService;
    }

    public void ProcessOrder(int orderId) { }
}

var builder = new IocContainerBuilder();

// 注册所有依赖
builder.RegisterType(typeof(IRepository<>), typeof(Repository<>));
builder.RegisterType<IUserService, UserService>();
builder.RegisterType<IOrderService, OrderService>();

var container = builder.Build();

// 自动解析所有依赖
var orderService = container.Resolve<IOrderService>();
```

#### 使用 `[IocResolve]` 特性指定构造函数

当类有多个构造函数时，使用 `[IocResolve]` 特性指定要使用的构造函数：

```csharp
public class MyService : IMyService
{
    public MyService() { }

    [IocResolve]
    public MyService(IDependency dep) { }
}
```

### 6. 自动注册

使用特性标记类，自动注册到容器：

```csharp
// 单例服务
[Singleton]
public class ConfigService : IConfigService
{
    public string GetConfig() => "config";
}

// 瞬态服务
[PerDependency]
public class TransientService : ITransientService
{
    public void DoWork() { }
}

// 容器范围服务
[ContainerScope]
public class ScopedService : IScopedService
{
    public void Process() { }
}

// 自动注册程序集中的所有标记类
var builder = new IocContainerBuilder();
builder.AutoRegister([Assembly.GetExecutingAssembly()]);

var container = builder.Build();

// 直接解析，无需手动注册
var config = container.Resolve<IConfigService>();
var transient = container.Resolve<ITransientService>();
var scoped = container.Resolve<IScopedService>();
```

### 7. 实例注册

```csharp
var builder = new IocContainerBuilder();

// 注册现有实例（作为单例）
var existingService = new MessageService();
builder.RegisterInstance<IMessageService>(existingService);

// 带名称注册实例
builder.RegisterInstance<IMessageService>(existingService, "Default");

var container = builder.Build();
```

### 8. 类型注册（非泛型）

```csharp
var builder = new IocContainerBuilder();

// 使用 Type 对象注册
builder.RegisterType(typeof(IMessageService), typeof(MessageService));

// 带名称注册
builder.RegisterType(typeof(IMessageService), typeof(MessageService), "Custom");

// 注册具体类型
builder.RegisterType<MessageService>();
builder.RegisterType<MessageService>("Named");
```

## API 快速参考

### IocContainerBuilder 扩展方法

| 方法 | 说明 | 生命周期 |
|------|------|----------|
| `RegisterType<TFrom, TTo>()` | 注册接口到实现 | PerDependency |
| `RegisterType<TFrom, TTo>(string name)` | 带名称注册 | PerDependency |
| `RegisterType<T>()` | 注册具体类型 | PerDependency |
| `RegisterType<T>(Func<T> factory)` | 使用工厂方法注册 | PerDependency |
| `RegisterScoped<TFrom, TTo>()` | 注册接口到实现 | ContainerScope |
| `RegisterScoped<TFrom, TTo>(string name)` | 带名称注册 | ContainerScope |
| `RegisterSingleton<TFrom, TTo>()` | 注册接口到实现 | Singleton |
| `RegisterSingleton<TFrom, TTo>(string name)` | 带名称注册 | Singleton |
| `RegisterInstance<T>(T instance)` | 注册实例 | Singleton |
| `RegisterInstance<T>(T instance, string name)` | 带名称注册实例 | Singleton |
| `AutoRegister(Assembly[] assemblies)` | 自动注册程序集 | 由特性决定 |

### 特性（Attributes）

| 特性 | 生命周期 | 用途 |
|------|----------|------|
| `[Singleton]` | Singleton | 标记类为单例，用于自动注册 |
| `[PerDependency]` | PerDependency | 标记类为瞬态，用于自动注册 |
| `[ContainerScope]` | ContainerScope | 标记类为容器范围，用于自动注册 |
| `[IocResolve]` | - | 标记要使用的构造函数 |

### IIocContainer 接口

| 方法 | 说明 |
|------|------|
| `Resolve<T>()` | 解析泛型类型的实例 |
| `Resolve<T>(string name)` | 通过名称解析泛型类型的实例 |
| `Resolve(Type type)` | 解析非泛型类型的实例 |
| `BeginContainerScope()` | 创建新的容器范围 |
| `Dispose()` | 释放容器资源 |

## 异常类型

| 异常类型 | 说明 |
|----------|------|
| `AuserIocException` | 基础异常类 |
| `NotRegisterTypeException` | 类型未注册时抛出 |
| `RegisteredTypeException` | 类型重复注册时抛出 |
| `CircularDependencyException` | 检测到循环依赖时抛出 |
| `IocResolveException` | 解析失败时抛出 |
| `IocObjectConfigurationException` | 配置错误时抛出 |
| `AutoRegisterException` | 自动注册失败时抛出 |
| `UnableToDetermineInterfaceException` | 无法确定接口时抛出 |

## 性能

AuserIoc 经过深度性能优化，适用于高性能场景：

### 性能优化

| 优化项 | 收益 |
|--------|------|
| ConditionalWeakTable 缓存 | 防止内存泄漏，GC 友好 |
| 泛型类型解析缓存 | O(n) → O(1) 查找 |
| 预编译构造函数调用 | 消除反射开销 |
| 工厂方法直接调用 | 避免 DynamicInvoke 开销 |
| 无锁单例访问 | 快速路径优化 |
| CustomAttributeData | 减少 GC 分配 |

### 性能基准

| 测试 | 耗时 | 阈值 |
|------|------|------|
| 100,000 次单例解析 | 42ms | < 500ms |
| 100,000 次瞬态解析 | 24ms | < 300ms |
| 100 线程 × 1,000 次并发解析 | 14ms | < 1000ms |
| 1,000 个泛型类型解析 | 1ms | < 300ms |
| 10,000 次工厂方法解析 | 5ms | < 150ms |

**关键指标:**
- 单次解析：~0.42μs（单例）
- 并发解析：100K 次解析耗时 14ms（100 线程）
- 泛型缓存：首次解析后 O(1)

### 压力测试

项目包含 15 个综合性能压力测试，涵盖：
- 大容量解析（100K+ 操作）
- 并发多线程访问（100 线程）
- 内存压力和泄漏检测
- 混合生命周期操作
- 工厂方法性能

所有测试均以显著优势通过定义的阈值。

## 参与贡献

1. Fork 本仓库
2. 新建 `Feat/xxx` 分支
3. 提交代码
4. 新建 Pull Request

## 许可证

本项目基于 MIT 许可证开源。
