namespace AuserIoc.Common.Attributes;

/// <summary>
/// 单例依赖注入的特性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SingletonAttribute : AutoRegisterAttribute
{
}
