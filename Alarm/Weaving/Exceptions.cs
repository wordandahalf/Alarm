using System.Reflection;
using Mono.Cecil;

namespace Alarm.Weaving;

public class MissingWeaveException(Assembly assembly, string weaveType) :
    InvalidOperationException($"Could not find Weave type '{weaveType}' in '{assembly.Location}'");
public class MissingWeaveAttributeException(Type weaveType) :
    InvalidOperationException($"Could not find Weave attribute for type '{weaveType.Name}' in '{weaveType.Assembly.Location}'");
public class BadWeaveException(Type weaveType) :
    InvalidOperationException($"Could not handle Weave-decorated type '{weaveType.Name}' in '{weaveType.Assembly.Location}'");
public class BadWeaveTargetException(Type weaveType, string weaveTarget) :
    InvalidOperationException($"Could not handle Weave target '{weaveTarget}' for type '{weaveType.Name}' in '{weaveType.Assembly.Location}'");
public class WeavingException(MethodInfo method, string message) :
    Exception($"Could not apply weaves on method {method.DeclaringType?.FullName}.{method.Name}: {message}");