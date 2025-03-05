using System.Reflection;
using Mono.Cecil;

namespace Alarm.Weaving;

public static class Reflection
{
    public static MethodDefinition? MethodDefinitionFromInfo(TypeDefinition type, MethodInfo info)
    {
        foreach (var method in type.Methods)
        {
            if (method.Name == info.Name &&
                method.Parameters.Count == info.GetParameters().Length &&
                method.Parameters.Select(p => p.ParameterType.FullName)
                    .SequenceEqual(info.GetParameters().Select(p => p.ParameterType.FullName)) &&
                method.ReturnType.FullName == info.ReturnType.FullName
               )
            {
                return method;
            }
        }

        return null;
    }
}