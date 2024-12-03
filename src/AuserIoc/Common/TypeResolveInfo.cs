using System.Reflection;

namespace AuserIoc.Common;

internal class TypeResolveInfo
{
    internal TypeResolveInfo(Type type, ConstructorInfo constructorInfo, ParameterInfo[] parameterInfos)
    {
        Type = type;
        ConstructorInfo = constructorInfo;
        ParameterInfos = parameterInfos;
    }
    internal Type Type { get; }
    internal ConstructorInfo ConstructorInfo { get; }
    internal ParameterInfo[] ParameterInfos { get; }
}
