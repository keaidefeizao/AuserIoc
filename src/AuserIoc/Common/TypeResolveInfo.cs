using System.Reflection;

namespace AuserIoc.Common;

internal class TypeResolveInfo
{
    private readonly Func<object[], object>? _compiledConstructor;

    internal TypeResolveInfo(Type type, ConstructorInfo constructorInfo, ParameterInfo[] parameterInfos)
    {
        Type = type;
        ConstructorInfo = constructorInfo;
        ParameterInfos = parameterInfos;

        // 预编译构造函数调用器（仅在有参数时编译，无参数的直接调用无需编译）
        if (parameterInfos.Length > 0)
        {
            _compiledConstructor = CompiledInvoker.GetOrAddConstructor(constructorInfo);
        }
    }

    internal Type Type { get; }
    internal ConstructorInfo ConstructorInfo { get; }
    internal ParameterInfo[] ParameterInfos { get; }

    /// <summary>
    /// 使用预编译的构造函数创建实例
    /// </summary>
    internal object Invoke(object?[]? parameters)
    {
        if (_compiledConstructor is not null)
        {
            return _compiledConstructor(parameters!);
        }

        // 无参数构造函数直接调用
        return ConstructorInfo.Invoke(null);
    }
}