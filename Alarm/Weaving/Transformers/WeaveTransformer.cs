using Mono.Cecil;

namespace Alarm.Weaving.Transformers;

public abstract class WeaveTransformer(TypeDefinition target, WeaveTransformer.ApplyPhase phase, int? priority)
{
    public readonly TypeDefinition Target = target;
    public readonly ApplyPhase Phase = phase;
    public readonly int? Priority = priority;
    
    public enum ApplyPhase
    {
        Source,
        Target,
        Override
    };
    
    public abstract void Apply();
}