using System.Collections.Concurrent;

namespace AuserIoc.Data;

internal class IocInstanceManager : ConcurrentDictionary<RegisterObject, object>
{
}