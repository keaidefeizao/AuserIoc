namespace AuserIoc.Common.Attributes;

/// <summary>
/// 瞬态依赖注入的特性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PerDependencyAttribute : AutoRegisterAttribute
{
}
