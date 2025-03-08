using Alarm.Weaving.Utils;
using Mono.Cecil;

namespace Alarm.Weaving.Transformers.Reference;

public class FieldReferenceTransformer(
    TypeDefinition target, int? priority, FieldReference oldReference, FieldReference newReference
) : MemberReferenceTransformer(target, priority, oldReference, newReference)
{
    public new readonly FieldReference OldReference = oldReference;
    public new readonly FieldReference NewReference = newReference;
    
    public override void Apply()
    {
        var decoratedMethods =
            Target.Methods.Where(it => it.HasCustomAttribute(typeof(TransformerAttribute)));
        
        foreach (var method in decoratedMethods) { Apply(method); }
    }
    
    private void Apply(MethodDefinition method)
    {
        var processor = method.Body.GetILProcessor();
        foreach (var i in method.Body.Instructions.ToList())
        {
            if (i.Operand is FieldReference reference && reference.EqualsReference(OldReference))
            {   
                processor.Replace(i, processor.Create(i.OpCode, NewReference));
            }
        }
    }
}