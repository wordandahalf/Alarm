using Mono.Cecil;

namespace Alarm.Weaving.Transformers.Reference;

public abstract class MemberReferenceTransformer(
    TypeDefinition target, int? priority,
    MemberReference oldReference, MemberReference newReference
) :
    WeaveTransformer(target, ApplyPhase.Source, priority)
{
    public readonly MemberReference OldReference = oldReference;
    public readonly MemberReference NewReference = newReference;
}