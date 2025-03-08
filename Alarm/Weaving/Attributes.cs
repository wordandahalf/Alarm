using Alarm.Weaving.Transformers;
using Alarm.Weaving.Transformers.Injection;
using Alarm.Weaving.Transformers.Reference;
using Alarm.Weaving.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Alarm.Weaving;

/// <summary>
/// Decorates a weave class
/// </summary>
/// <param name="type">The fully qualified name of the target type</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class Weave(string type) : Attribute
{
    public readonly string Type = type;
}

public abstract class TransformerAttribute(int? priority) : Attribute
{
    protected TransformerAttribute() : this(null) {}

    public readonly int? Priority = priority < 0 ? null : priority;
    
    public abstract void AddTransformers(WeaveTarget source, WeaveTarget target, IMemberDefinition decorated);
}

/// <summary>
/// Decorates an injection method
/// </summary>
/// <param name="at">The location where this method's bytecode is injected</param>
/// <param name="name">The name of the target method</param>
[AttributeUsage(AttributeTargets.Method)]
public class Inject(Inject.Location at, string? name = null, int priority = -1) : TransformerAttribute(priority)
{
    public readonly Location At = at;
    public readonly string? Name = name;
    
    public enum Location
    {
        Head, Tail
    }

    public override void AddTransformers(WeaveTarget source, WeaveTarget target, IMemberDefinition decorated)
    {
        var sourceMethod = (MethodDefinition) decorated;
        var targetMethod = target.Definition.Methods.First(x => x.Name == decorated.Name);
        
        target.AddTransformers(new InjectMethodTransformer(Priority, sourceMethod, targetMethod, At));
    }
}

/// <summary>
/// Decorates an overwrite method
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class Overwrite(int priority = -1) : TransformerAttribute(priority)
{
    public override void AddTransformers(WeaveTarget source, WeaveTarget target, IMemberDefinition decorated)
    {
        var sourceMethod = (MethodDefinition) decorated;
        var targetMethod = target.Definition.Methods.First(x => x.Name == decorated.Name);
        
        target.AddTransformers(new OverwriteMethodTransformer(Priority, sourceMethod, targetMethod));
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
public class Shadow : TransformerAttribute
{
    public override void AddTransformers(WeaveTarget source, WeaveTarget target, IMemberDefinition decorated)
    {
        MemberReferenceTransformer transformer = decorated switch
        {
            MethodDefinition method =>
                new MethodReferenceTransformer(
                    source.Definition, Priority,
                    method.ToMethodReference(),
                    target.Definition.Methods.First(x => x.Name == method.Name).ToMethodReference()
                ),
            FieldDefinition field =>
                new FieldReferenceTransformer(
                    source.Definition, Priority,
                    field.ToFieldReference(),
                    target.Definition.Fields.First(x => x.Name == field.Name).ToFieldReference()
                ),
            _ => throw new NotImplementedException($"Cannot handle member '{decorated.FullName}' ({decorated.GetType().Name}) decorated with Shadow attribute")
        };
        
        source.AddTransformers(transformer);
    }
}