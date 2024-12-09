# AuserIoc

## 介绍
AuserIoc 是一个轻量级的依赖注入（IOC）容器，旨在提供高效、易用的对象管理和依赖关系解析功能。它适用于需要灵活管理对象生命周期和依赖注入的 .NET 应用程序。

## 功能特色
- **依赖注入支持**：支持构造函数注入和方法参数注入。
- **生命周期管理**：支持单例、瞬态等多种对象生命周期。
- **模块化设计**：通过构建器模式和扩展方法轻松自定义。
- **异常处理**：清晰的异常定义，便于调试和错误排查。

## 软件架构
支持net4.6.2、net4.7.2、net4.8.1、net6、net8、net9

## 安装
    dotnet add package AuserIoc

## 使用方法

### 注册对象
``` csharp
var containerBuilder = new IocContainerBuilder();
containerBuilder.Register<IService, ServiceImplementation>();
var container = containerBuilder.Build();
```

### 获取对象实例
~~~csharp
var service = container.Resolve<IService>();
service.DoSomething();
~~~

### 生命周期管理
支持多种生命周期：
- **单例**: 多个容器内共享一个实例。
- **瞬态**: 每次请求创建新实例。
- **容器范围**：容器内共享一个实例。

## 参与贡献
1.  Fork 本仓库
2.  新建 Feat_xxx 分支
3.  提交代码
4.  新建 Pull Request