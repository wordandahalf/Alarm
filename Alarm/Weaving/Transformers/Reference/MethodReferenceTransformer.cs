using Alarm.Weaving.Utils;
using Mono.Cecil;

namespace Alarm.Weaving.Transformers.Reference;

public class MethodReferenceTransformer(
    TypeDefinition target, int? priority, MethodReference oldReference, MethodReference newReference
) : MemberReferenceTransformer(target, priority, oldReference, newReference)
{
    public new readonly MethodReference OldReference = oldReference;
    public new readonly MethodReference NewReference = newReference;
    
    public override void Apply()
    {
        var decoratedMethods =
            Target.Methods.Where(it => it.HasCustomAttribute(typeof(TransformerAttribute)))
                .Where(it => !it.IsAbstract);
        
        foreach (var method in decoratedMethods) { Apply(method); }
    }


    private void Apply(MethodDefinition method)
    {
        var processor = method.Body.GetILProcessor();
        foreach (var i in method.Body.Instructions.ToList())
        {
            if (i.Operand is MethodReference reference && reference.EqualsReference(OldReference))
            {   
                processor.Replace(i, processor.Create(i.OpCode, NewReference));
            }
        }
    }
}