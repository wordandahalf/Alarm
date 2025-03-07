using Mono.Cecil;

namespace Alarm.Weaving.Transformers.Injection;

public abstract class BinaryMethodTransformer(
    WeaveTransformer.ApplyPhase phase,
    int? priority,
    MethodDefinition source,
    MethodDefinition target
) : WeaveTransformer(target.DeclaringType, phase, priority)
{
    public readonly MethodDefinition SourceMethod = source;
    public readonly MethodDefinition TargetMethod = target;
}