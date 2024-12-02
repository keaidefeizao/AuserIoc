using AuserIoc.Exceptions;

namespace AuserIoc.Data;

internal class IocObjectManage(IReadOnlyDictionary<Type, IocObject> IocObjectMap)
{
    internal IocObject this[Type type]
    {
        get
        {
            return Get(type);
        }
    }

    internal IReadOnlyDictionary<Type, IocObject> Map => IocObjectMap;

    private IocObject Get(Type type)
    {
        if (IocObjectMap.ContainsKey(type))
        {
            return IocObjectMap[type];
        }

        if (type.IsGenericType)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();

            foreach (var item in IocObjectMap)
            {
                if (item.Key == genericTypeDefinition)
                {
                    return item.Value;
                }

                if (item.Value.ParentType == genericTypeDefinition)
                {
                    return item.Value;
                }
            }
        }
        else
        {
            foreach (var item in IocObjectMap)
            {
                if (item.Value.ParentType == type)
                {
                    return item.Value;
                }
            }
        }

        throw new NotRegisterTypeException(type);
    }
}
