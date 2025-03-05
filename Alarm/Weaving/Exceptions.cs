using System.Reflection;
using Mono.Cecil;

namespace Alarm.Weaving;

public class WeavingException(MethodInfo method, string message) :
    Exception($"Could not apply weaves on method {method.DeclaringType?.FullName}.{method.Name}: {message}");
public class TooManyInjectionsException(MethodInfo method) :
    WeavingException(method, "too many injections");