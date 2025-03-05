using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Alarm.Weaving;

public static class Inject
{
    private static readonly ModuleDefinition AlarmModule =
        AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location).MainModule;

    public static void Overwrite(ModuleDefinition game, string modifyTypeName, string modifyMethodName, Type callType,
        string callMethodName)
    {
        var type = game.GetType(modifyTypeName)!;
        var method = type.Methods.SingleOrDefault(it => it.Name == modifyMethodName)!;
        
        var callMethodExternal = AlarmModule.ImportReference(callType.GetMethod(callMethodName))!;
        var callMethod = game.ImportReference(callMethodExternal);
        
        var modifyBody = method.Body!;
        var instr = modifyBody.GetILProcessor();
        
        instr.Clear();
        instr.Append(Instruction.Create(OpCodes.Call, callMethod));
        instr.Append(Instruction.Create(OpCodes.Ret));
    }
    
    public static void CallAtTail(ModuleDefinition game, string modifyTypeName, string modifyMethodName, Type callType,
        string callMethodName)
    {
        var type = game.GetType(modifyTypeName)!;
        var method = type.Methods.SingleOrDefault(it => it.Name == modifyMethodName)!;
        
        var callMethodExternal = AlarmModule.ImportReference(callType.GetMethod(callMethodName))!;
        var callMethod = game.ImportReference(callMethodExternal);
        
        var modifyBody = method.Body!;
        var instr = modifyBody.GetILProcessor();
        var tail = modifyBody.Instructions.Reverse().First(it => it.OpCode == OpCodes.Ret);
        
        instr.InsertBefore(tail, Instruction.Create(OpCodes.Call, callMethod));
    }
}