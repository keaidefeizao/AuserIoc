namespace AuserIoc.Common.Attributes;

/// <summary>
/// 容器范围依赖注入的特性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ContainerScopeAttribute : AutoRegisterAttribute
{
}
