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
        var sourceInstructions = SourceMethod.Body.Instructions;
        var targetInstructions = TargetMethod.Body.Instructions;
        
        var processor = TargetMethod.Body.GetILProcessor();
        var position = At switch
        {
            Inject.Location.Head => targetInstructions.First(),
            Inject.Location.Tail => targetInstructions.Last(it => it.OpCode == OpCodes.Ret),
            _ => throw new NotImplementedException()
        };

        var targetVariables = TargetMethod.Body.Variables;
        var offset = (byte) targetVariables.Count;
        var sourceVariables = SourceMethod.Body.Variables;

        foreach (var v in sourceVariables)
            targetVariables.Add(v.UpdateReference(TargetMethod));
        
        // we need to drop the trailing RET in the source method,
        // but existing branches need to be updated.
        var updatedTargets = new Dictionary<Instruction, Instruction>();
        updatedTargets[targetInstructions.Last()] = sourceInstructions.First();
        updatedTargets[sourceInstructions.Last()] = targetInstructions.Last();

        var croppedSource = sourceInstructions.ToList().Take(sourceInstructions.Count - 1);
        
        // patch try/catch/finally blocks that start or end at the injection position
        foreach (var h in TargetMethod.Body.ExceptionHandlers)
        {
            if (h.TryStart == position) h.TryStart = sourceInstructions.Last();
            if (h.TryEnd == position)   h.TryEnd = sourceInstructions.First();
            if (h.HandlerStart == position) h.HandlerStart = sourceInstructions.Last();
            if (h.HandlerEnd == position)   h.HandlerEnd = sourceInstructions.First();
        }
        
        foreach (var i in croppedSource)
        {
            processor.InsertBefore(
                position, 
                // we need to do the usual reference updating, but also fixup
                // any load/store of local variables.
                i.UpdateReference(TargetMethod).UpdateLocalVar(TargetMethod, offset)
            );
        }

        // perform the branch target updates
        foreach (var i in targetInstructions.ToList())
            processor.Replace(i, i.UpdateBranchTargets(updatedTargets));
    }
}