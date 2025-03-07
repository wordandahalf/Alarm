using Mono.Cecil;

namespace Alarm.Weaving.Transformers;

public abstract class WeaveTransformer(TypeDefinition target, WeaveTransformer.ApplyPhase phase, int? priority)
{
    public TypeDefinition Target = target;
    public ApplyPhase Phase = phase;
    public int? Priority = priority;
    
    public enum ApplyPhase
    {
        Source,
        Target,
        Override
    };
    
    public abstract void Apply();
}