namespace AuserIoc.Common.Attributes;

/// <summary>
/// 标记需要解析的对象
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class IocResolveAttribute : Attribute
{
}
