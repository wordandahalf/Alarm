using System.Reflection;
using Mono.Cecil;

namespace Alarm.Weaving.Utils;

public static class Reflection
{
    public static bool EqualsReference(this FieldReference left, FieldReference right)
    {
        return left.Name == right.Name
               && left.DeclaringType.FullName == right.DeclaringType.FullName
               && left.FieldType.FullName == right.FieldType.FullName;
    }

    public static bool EqualsReference(this MethodReference left, MethodReference right)
    {
        return left.Name == right.Name
               && left.DeclaringType.FullName == right.DeclaringType.FullName
               && left.ReturnType.FullName == right.ReturnType.FullName;
    }
    
    public static FieldReference ToFieldReference(this FieldDefinition fieldDefinition)
    {
        var fieldReference = new FieldReference(
            fieldDefinition.Name,
            fieldDefinition.FieldType,
            fieldDefinition.DeclaringType
        );

        return fieldReference;
    }
    
    public static MethodReference ToMethodReference(this MethodDefinition methodDefinition)
    {
        var methodReference = new MethodReference(
            methodDefinition.Name,
            methodDefinition.ReturnType,
            methodDefinition.DeclaringType
        )
        {
            HasThis = methodDefinition.HasThis,
            ExplicitThis = methodDefinition.ExplicitThis,
            CallingConvention = methodDefinition.CallingConvention
        };

        // Copy parameters
        foreach (var parameter in methodDefinition.Parameters)
        {
            methodReference.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, parameter.ParameterType));
        }

        // Copy generic parameters
        foreach (var genericParameter in methodDefinition.GenericParameters)
        {
            methodReference.GenericParameters.Add(new GenericParameter(genericParameter.Name, methodReference));
        }

        return methodReference;
    }
    
    public static MethodDefinition? GetMethod(this TypeDefinition type, MethodInfo info)
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

    public static FieldDefinition? GetField(this TypeDefinition type, FieldInfo fieldInfo)
    {
        foreach (var field in type.Fields)
        {
            if (field.Name == fieldInfo.Name && field.FieldType.FullName == fieldInfo.FieldType.FullName)
            {
                return field;
            }
        }

        return null;
    }

    public static bool HasCustomAttribute(this MethodDefinition method, Type attributeType)
    {
        return method.CustomAttributes
            .Any(it => it.AttributeType.Resolve().BaseType.FullName == attributeType.FullName);
    }
}