using Alarm.Weaving.Utils;
using API.Weaves;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Alarm.Weaving.Transformers.Injection;

public class InjectMethodTransformer(
    int? priority, MethodDefinition source, MethodDefinition target, Inject.Location at
) : BinaryMethodTransformer(ApplyPhase.Target, priority, source, target)
{
    public readonly Inject.Location At = at;

    public override void Apply()
    {
        var processor = TargetMethod.Body.GetILProcessor();
        var position = At switch
        {
            Inject.Location.Head => processor.Body.Instructions.First(),
            Inject.Location.Tail => processor.Body.Instructions.Last(it => it.OpCode == OpCodes.Ret),
            _ => throw new NotImplementedException()
        };

        var targetVariables = TargetMethod.Body.Variables;
        var offset = (byte) targetVariables.Count;
        var sourceVariables = SourceMethod.Body.Variables;

        foreach (var v in sourceVariables)
            targetVariables.Add(v.UpdateReference(TargetMethod));
        
        foreach (var i in SourceMethod.Body.Instructions)
        {
            processor.InsertBefore(
                position, 
                // we need to do the usual reference updating, but also fixup
                // any load/store of local variables.
                i.UpdateReference(TargetMethod).UpdateLocalVar(TargetMethod, offset)
            );
        }
    }
}