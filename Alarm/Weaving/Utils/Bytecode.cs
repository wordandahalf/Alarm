using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Alarm.Weaving.Utils;

/// <summary>
/// Utility class for bytecode-related methods.
/// </summary>
public static class Bytecode
{
    private enum LocalVarOp
    {
        Store,
        Load
    }

    private static LocalVarOp? GetLocalVarOp(this Instruction instruction)
    {
        return instruction.OpCode.Code switch
        {
            Code.Stloc_0 => LocalVarOp.Store,
            Code.Stloc_1 => LocalVarOp.Store,
            Code.Stloc_2 => LocalVarOp.Store,
            Code.Stloc_3 => LocalVarOp.Store,
            Code.Stloc_S => LocalVarOp.Store,
            Code.Ldloc_0 => LocalVarOp.Load,
            Code.Ldloc_1 => LocalVarOp.Load,
            Code.Ldloc_2 => LocalVarOp.Load,
            Code.Ldloc_3 => LocalVarOp.Load,
            Code.Ldloc_S => LocalVarOp.Load,
            _ => null,
        };
    }

    private static byte? GetLocalVarIndex(this Instruction instruction)
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
            _ => null
        };
    }

    public static Instruction UpdateReference(this Instruction instruction, MethodDefinition target)
    {
        return instruction.Operand switch
        {
            MethodReference method =>
                Instruction.Create(instruction.OpCode, target.Module.ImportReference(method)),
            TypeReference type =>
                Instruction.Create(instruction.OpCode, target.Module.ImportReference(type)),
            FieldReference field =>
                Instruction.Create(instruction.OpCode, target.Module.ImportReference(field)),
            _ => instruction
        };
    }

    public static Instruction UpdateLocalVar(this Instruction instruction, MethodDefinition method, byte offset)
    {
        // don't need to do anything if no offset
        if (offset == 0) return instruction;
        
        return instruction.GetLocalVarOp() switch
        {
            LocalVarOp.Load => instruction.OffsetLoadLocal(method, offset),
            LocalVarOp.Store => instruction.OffsetStoreLocal(method, offset),
            _ => instruction
        };
    }

    private static Instruction OffsetLoadLocal(this Instruction instruction, MethodDefinition method, byte offset)
    {
        var baseOffset = instruction.GetLocalVarIndex() ?? throw new InvalidOperationException();
        var finalOffset = (byte)(baseOffset + offset);
        
        return finalOffset switch
        {
            0 => Instruction.Create(OpCodes.Ldloc_0),
            1 => Instruction.Create(OpCodes.Ldloc_1),
            2 => Instruction.Create(OpCodes.Ldloc_2),
            3 => Instruction.Create(OpCodes.Ldloc_3),
            _ => Instruction.Create(OpCodes.Ldloc_S, method.Body.Variables[finalOffset]),
        };
    }

    private static Instruction OffsetStoreLocal(this Instruction instruction, MethodDefinition method, byte offset)
    {
        var baseOffset = instruction.GetLocalVarIndex() ?? throw new InvalidOperationException();
        var finalOffset = (byte)(baseOffset + offset);
        
        return finalOffset switch
        {
            0 => Instruction.Create(OpCodes.Stloc_0),
            1 => Instruction.Create(OpCodes.Stloc_1),
            2 => Instruction.Create(OpCodes.Stloc_2),
            3 => Instruction.Create(OpCodes.Stloc_3),
            _ => Instruction.Create(OpCodes.Stloc_S, method.Body.Variables[finalOffset]),
        };
    }
}