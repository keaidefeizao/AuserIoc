# AuserIoc

## Description
AuserIoc is a lightweight dependency injection (IOC) container designed to provide efficient, easy-to-use object management and dependency resolution capabilities. It is suitable for.NET applications that require flexible object lifecycle management and dependency injection.

## Featur
- **Dependency Injection support** : Constructor injection and method parameter injection are supported.
- **Life Cycle Management** : Supports multiple object life cycles such as singleton and transient.
- **Modular Design** : Easy customization with builder patterns and extension methods.
- **Exception Handling** : Clear exception definition, easy to debug and error troubleshooting.

## Software Architecture
Supports net4.6.2, net4.7.2, net4.8.1, net6, net8, and net9

## Installation
    dotnet add package AuserIoc
    
## Usage

### Registered object
``` csharp
var containerBuilder = new IocContainerBuilder();
containerBuilder.Register<IService, ServiceImplementation>();
var container = containerBuilder.Build();
```

### Get object instance
~~~csharp
var service = container.Resolve<IService>();
service.DoSomething();
~~~

## Contribution
1.  Fork the repository
2.  Create Feat_xxx branch
3.  Commit your code
4.  Create Pull Request
