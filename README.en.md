# AuserIoc

[![NuGet](https://img.shields.io/nuget/v/AuserIoc.svg)](https://www.nuget.org/packages/AuserIoc)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Description

AuserIoc is a lightweight dependency injection (IOC) container designed to provide efficient and easy-to-use object management and dependency resolution capabilities for .NET applications. It features a modular design with support for multiple object lifecycle management patterns.

## Features

- **Multiple Injection Styles**: Constructor injection and method parameter injection with `[IocResolve]` attribute for precise control
- **Flexible Lifecycle Management**:
  - **Singleton**: Share a single instance throughout the application lifetime
  - **PerDependency**: Create a new instance for each request
  - **ContainerScope**: Share instances within the same container scope
- **Auto-Registration**: Scan assemblies and automatically register types marked with attributes (`[Singleton]`, `[PerDependency]`, `[ContainerScope]`)
- **Generic Type Support**: Support for both open and closed generic type registration and resolution
- **Named Registration**: Distinguish multiple implementations of the same interface by name
- **Factory Method Registration**: Create instances using custom factory methods
- **Circular Dependency Detection**: Automatically detect and throw circular dependency exceptions at runtime
- **Comprehensive Exception Handling**: Multiple specialized exception types for easy debugging
- **Thread-Safe**: All resolution operations are thread-safe for high-concurrency scenarios

## Supported Frameworks

- .NET Framework 4.6.2 / 4.7.2 / 4.8.1
- .NET 6.0
- .NET 8.0
- .NET 9.0

## Installation

```bash
dotnet add package AuserIoc
```

## Quick Start

### Basic Registration and Resolution

```csharp
using AuserIoc;

// Define an interface
public interface IMessageService
{
    void SendMessage(string message);
}

// Implement the interface
public class MessageService : IMessageService
{
    public void SendMessage(string message)
    {
        Console.WriteLine($"Message: {message}");
    }
}

// Configure the container
var builder = new IocContainerBuilder();
builder.RegisterType<IMessageService, MessageService>();

// Build the container
var container = builder.Build();

// Resolve the service
var service = container.Resolve<IMessageService>();
service.SendMessage("Hello, AuserIoc!");
```

## Detailed Usage Guide

### 1. Lifecycle Management

#### PerDependency - Create a new instance for each resolution

```csharp
var builder = new IocContainerBuilder();

// Register transient service
builder.RegisterType<IMessageService, MessageService>();

var container = builder.Build();

var service1 = container.Resolve<IMessageService>();
var service2 = container.Resolve<IMessageService>();

// service1 and service2 are different instances
Console.WriteLine(ReferenceEquals(service1, service2)); // False
```

#### Singleton - Share one instance throughout the lifetime

```csharp
var builder = new IocContainerBuilder();

// Register singleton service
builder.RegisterSingleton<IMessageService, MessageService>();

var container = builder.Build();

var service1 = container.Resolve<IMessageService>();
var service2 = container.Resolve<IMessageService>();

// service1 and service2 are the same instance
Console.WriteLine(ReferenceEquals(service1, service2)); // True
```

#### ContainerScope - Share instances within container scope

```csharp
var builder = new IocContainerBuilder();

// Register scoped service
builder.RegisterScoped<IMessageService, MessageService>();

var container = builder.Build();

// Resolve within the same container
var service1 = container.Resolve<IMessageService>();
var service2 = container.Resolve<IMessageService>();
Console.WriteLine(ReferenceEquals(service1, service2)); // True

// Create a new container scope
// Note: Use 'using' statement to ensure resources are released when scope ends
using (var scope = container.BeginContainerScope())
{
    var service3 = scope.Resolve<IMessageService>();
    // Instances in different scopes are different
    Console.WriteLine(ReferenceEquals(service1, service3)); // False
}
// scope.Dispose() is automatically called here
```

**Note**: `BeginContainerScope()` returns an object that implements `IDisposable`. When using container scopes, it is recommended to wrap with a `using` statement to ensure proper resource disposal when the scope ends.

### 2. Named Registration

When multiple implementations exist for the same interface, use named registration to distinguish them:

```csharp
public interface ILogger
{
    void Log(string message);
}

public class FileLogger : ILogger { }
public class DatabaseLogger : ILogger { }
public class ConsoleLogger : ILogger { }

var builder = new IocContainerBuilder();

// Register multiple implementations with names
builder.RegisterType<ILogger, FileLogger>("File");
builder.RegisterType<ILogger, DatabaseLogger>("Database");
builder.RegisterType<ILogger, ConsoleLogger>("Console");

var container = builder.Build();

// Resolve specific implementation by name
var fileLogger = container.Resolve<ILogger>("File");
var dbLogger = container.Resolve<ILogger>("Database");
```

### 3. Factory Method Registration

#### Factory method without parameters

```csharp
var builder = new IocContainerBuilder();

builder.RegisterType<IMessageService>(() => new MessageService());

var container = builder.Build();
var service = container.Resolve<IMessageService>();
```

#### Factory method with container parameter

```csharp
var builder = new IocContainerBuilder();

builder.RegisterSingleton<IMessageService>(container =>
{
    // Can resolve other dependencies from the container
    var config = container.Resolve<IConfiguration>();
    return new MessageService(config);
});
```

#### Using Delegate factory method

```csharp
var builder = new IocContainerBuilder();

builder.RegisterType<IMessageService>((IMessageService service) =>
{
    return new MessageService();
});
```

### 4. Generic Type Registration

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

// Register open generic type
builder.RegisterType(typeof(IRepository<>), typeof(Repository<>));

var container = builder.Build();

// Resolve closed generic types
var userRepository = container.Resolve<IRepository<User>>();
var orderRepository = container.Resolve<IRepository<Order>>();
```

### 5. Constructor Parameter Injection

AuserIoc supports automatic constructor parameter resolution:

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

// Register all dependencies
builder.RegisterType(typeof(IRepository<>), typeof(Repository<>));
builder.RegisterType<IUserService, UserService>();
builder.RegisterType<IOrderService, OrderService>();

var container = builder.Build();

// All dependencies are automatically resolved
var orderService = container.Resolve<IOrderService>();
```

#### Using `[IocResolve]` Attribute to Specify Constructor

When a class has multiple constructors, use the `[IocResolve]` attribute to specify which one to use:

```csharp
public class MyService : IMyService
{
    public MyService() { }

    [IocResolve]
    public MyService(IDependency dep) { }
}
```

### 6. Auto-Registration

Mark classes with attributes for automatic registration:

```csharp
// Singleton service
[Singleton]
public class ConfigService : IConfigService
{
    public string GetConfig() => "config";
}

// Transient service
[PerDependency]
public class TransientService : ITransientService
{
    public void DoWork() { }
}

// Scoped service
[ContainerScope]
public class ScopedService : IScopedService
{
    public void Process() { }
}

// Auto-register all marked types in the assembly
var builder = new IocContainerBuilder();
builder.AutoRegister([Assembly.GetExecutingAssembly()]);

var container = builder.Build();

// Resolve directly without manual registration
var config = container.Resolve<IConfigService>();
var transient = container.Resolve<ITransientService>();
var scoped = container.Resolve<IScopedService>();
```

### 7. Instance Registration

```csharp
var builder = new IocContainerBuilder();

// Register existing instance (as singleton)
var existingService = new MessageService();
builder.RegisterInstance<IMessageService>(existingService);

// Register instance with name
builder.RegisterInstance<IMessageService>(existingService, "Default");

var container = builder.Build();
```

### 8. Type Registration (Non-Generic)

```csharp
var builder = new IocContainerBuilder();

// Register using Type objects
builder.RegisterType(typeof(IMessageService), typeof(MessageService));

// Register with name
builder.RegisterType(typeof(IMessageService), typeof(MessageService), "Custom");

// Register concrete type
builder.RegisterType<MessageService>();
builder.RegisterType<MessageService>("Named");
```

## API Quick Reference

### IocContainerBuilder Extension Methods

| Method | Description | Lifecycle |
|--------|-------------|-----------|
| `RegisterType<TFrom, TTo>()` | Register interface to implementation | PerDependency |
| `RegisterType<TFrom, TTo>(string name)` | Register with name | PerDependency |
| `RegisterType<T>()` | Register concrete type | PerDependency |
| `RegisterType<T>(Func<T> factory)` | Register with factory method | PerDependency |
| `RegisterScoped<TFrom, TTo>()` | Register interface to implementation | ContainerScope |
| `RegisterScoped<TFrom, TTo>(string name)` | Register with name | ContainerScope |
| `RegisterSingleton<TFrom, TTo>()` | Register interface to implementation | Singleton |
| `RegisterSingleton<TFrom, TTo>(string name)` | Register with name | Singleton |
| `RegisterInstance<T>(T instance)` | Register instance | Singleton |
| `RegisterInstance<T>(T instance, string name)` | Register instance with name | Singleton |
| `AutoRegister(Assembly[] assemblies)` | Auto-register assemblies | Determined by attribute |

### Attributes

| Attribute | Lifecycle | Purpose |
|-----------|-----------|---------|
| `[Singleton]` | Singleton | Mark class as singleton for auto-registration |
| `[PerDependency]` | PerDependency | Mark class as transient for auto-registration |
| `[ContainerScope]` | ContainerScope | Mark class as scoped for auto-registration |
| `[IocResolve]` | - | Mark constructor to use for resolution |

### IIocContainer Interface

| Method | Description |
|--------|-------------|
| `Resolve<T>()` | Resolve instance of generic type |
| `Resolve<T>(string name)` | Resolve instance by name |
| `Resolve(Type type)` | Resolve instance of non-generic type |
| `BeginContainerScope()` | Create a new container scope |
| `Dispose()` | Dispose container resources |

## Exception Types

| Exception Type | Description |
|----------------|-------------|
| `AuserIocException` | Base exception class |
| `NotRegisterTypeException` | Thrown when type is not registered |
| `RegisteredTypeException` | Thrown when type is already registered |
| `CircularDependencyException` | Thrown when circular dependency is detected |
| `IocResolveException` | Thrown when resolution fails |
| `IocObjectConfigurationException` | Thrown for configuration errors |
| `AutoRegisterException` | Thrown when auto-registration fails |
| `UnableToDetermineInterfaceException` | Thrown when interface cannot be determined |

## Performance

AuserIoc has been heavily optimized for high-performance scenarios:

### Performance Optimizations

| Optimization | Benefit |
|--------------|---------|
| ConditionalWeakTable Cache | Prevents memory leaks, GC-friendly |
| Generic Type Resolution Cache | O(n) → O(1) lookup |
| Pre-compiled Constructor Calls | Eliminates reflection overhead |
| Factory Method Direct Calls | Avoids DynamicInvoke overhead |
| Lock-free Singleton Access | Fast path optimization |
| CustomAttributeData Usage | Reduces GC allocations |

### Performance Benchmarks

| Test | Time | Threshold |
|------|------|-----------|
| 100,000 Singleton Resolves | 42ms | < 500ms |
| 100,000 PerDependency Resolves | 24ms | < 300ms |
| 100 Threads × 1,000 Concurrent Resolves | 14ms | < 1000ms |
| 1,000 Generic Type Resolves | 1ms | < 300ms |
| 10,000 Factory Method Resolves | 5ms | < 150ms |

**Key Metrics:**
- Single resolve: ~0.42μs (singleton)
- Concurrent resolution: 100K resolves in 14ms (100 threads)
- Generic type cache: O(1) after first resolution

### Stress Testing

The project includes 15 comprehensive performance stress tests covering:
- High-volume resolution (100K+ operations)
- Concurrent multi-threaded access (100 threads)
- Memory pressure and leak detection
- Mixed lifecycle operations
- Factory method performance

All tests pass with significant margin under defined thresholds.

## Contributing

1. Fork this repository
2. Create a `Feat/xxx` branch
3. Commit your changes
4. Create a Pull Request

## License

This project is licensed under the MIT License.
