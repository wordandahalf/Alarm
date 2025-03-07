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
        Console.WriteLine($"Transforming source reference {OldReference.FullName} to {NewReference.FullName}");

        foreach (var method in Target.Methods.Where(it => it.HasCustomAttribute(typeof(TransformerAttribute))))
        {
            Apply(method);
        }
    }


    private void Apply(MethodDefinition method)
    {
        var processor = method.Body.GetILProcessor();

        foreach (var i in method.Body.Instructions.ToList())
        {
            if (i.Operand is FieldReference reference)
            {
                if (!reference.EqualsReference(OldReference)) continue;
                var replacement = processor.Create(i.OpCode, NewReference); 
                Console.WriteLine($"Transforming reference in instruction {i}: {replacement}");
                
                processor.Replace(i, replacement);
            }
        }
    }
}