using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Alarm.Weaving.Transformers.Injection;

public class InjectMethodTransformer(
    int? priority, MethodDefinition source, MethodDefinition target, Inject.Location at
) : BinaryMethodTransformer(ApplyPhase.Target, priority, source, target)
{
    public Inject.Location At = at;

    private enum LocalVarOp
    {
        Store, Load
    }

    private LocalVarOp? GetLocalVarOp(Instruction instruction)
    {
        return instruction.OpCode.Code switch
        {
            Code.Stloc_0 => LocalVarOp.Store,
            Code.Ldloc_0 => LocalVarOp.Load,
            Code.Stloc_1 => LocalVarOp.Store,
            Code.Ldloc_1 => LocalVarOp.Load,
            Code.Stloc_2 => LocalVarOp.Store,
            Code.Ldloc_2 => LocalVarOp.Load,
            Code.Stloc_3 => LocalVarOp.Store,
            Code.Ldloc_3 => LocalVarOp.Load,
            Code.Stloc_S => LocalVarOp.Store,
            Code.Ldloc_S => LocalVarOp.Load,
            _ => null,
        };
    }
    
    private byte LocalIndexFromInstruction(Instruction instruction)
    {
        return instruction.OpCode.Code switch
        {
            Code.Stloc_0 => 0,
            Code.Ldloc_0 => 0,
            Code.Stloc_1 => 1,
            Code.Ldloc_1 => 1,
            Code.Stloc_2 => 2,
            Code.Ldloc_2 => 2,
            Code.Stloc_3 => 3,
            Code.Ldloc_3 => 3,
            Code.Stloc_S => (byte)instruction.Operand,
            Code.Ldloc_S => (byte)instruction.Operand,
            _ => throw new NotImplementedException(),
        };
    }
    
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
        var sourceVariables = source.Body.Variables;
        var localOffset = targetVariables.Count;

        foreach (var v in sourceVariables)
            targetVariables.Add(new VariableDefinition(TargetMethod.Module.ImportReference(v.VariableType)));
        
        foreach (var i in source.Body.Instructions)
        {
            // todo: common method
            var patched = i.Operand switch
            {
                MethodReference methodReference =>
                    processor.Create(i.OpCode, TargetMethod.Module.ImportReference(methodReference)),
                TypeReference typeReference =>
                    processor.Create(i.OpCode, TargetMethod.Module.ImportReference(typeReference)),
                FieldReference fieldReference =>
                    processor.Create(i.OpCode, TargetMethod.Module.ImportReference(fieldReference)),
                _ => i
            };

            patched = GetLocalVarOp(patched) switch
            {
                LocalVarOp.Load =>
                    processor.Create(OpCodes.Ldloc_S, (byte)(LocalIndexFromInstruction(patched) + localOffset)),
                LocalVarOp.Store =>
                    processor.Create(OpCodes.Stloc_S, (byte)(LocalIndexFromInstruction(patched) + localOffset)),
                null => patched,
                _ => throw new NotImplementedException(),
            };
            
            processor.InsertBefore(position, patched);
        }
    }
}