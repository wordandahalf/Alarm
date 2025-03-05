using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Alarm.Weaving;

public static class Weaving
{
    private static readonly ModuleDefinition AlarmModule =
        AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location).MainModule;

    public static void HandleClass(ModuleDefinition game, ModuleDefinition weaveModule, Type weave)
    {
        var weaveAttr = Attribute.GetCustomAttribute(weave, typeof(Weave), false) as Weave;

        Console.WriteLine($"Found weave '{weave.FullName}'.");
        
        foreach (var method in weave.GetRuntimeMethods())
        {
            var injections =
                Attribute.GetCustomAttributes(method).Where(it => it is IInjectionMethod)
                    .ToArray();

            if (injections.Length == 0) continue;
            if (injections.Length > 1) throw new TooManyInjectionsException(method);
            
            // todo: error handling
            HandleMethod(
                weaveAttr!, game,
                (injections[0] as IInjectionMethod)!,
                Reflection.MethodDefinitionFromInfo(weaveModule.GetType(weave.FullName), method)!
            );
        }
    }

    private static void HandleMethod(Weave weave, ModuleDefinition game, IInjectionMethod injection,
        MethodDefinition weaveMethod)
    {
        Console.WriteLine($"Handling '{injection.GetType().FullName}' for method '{weaveMethod.FullName}'.");
        
        var targetType = game.GetType(weave.Name!)!;
        var targetMethod = targetType.Methods.SingleOrDefault(it => it.Name == weaveMethod.Name)!;
        
        var externalCall = AlarmModule.ImportReference(weaveMethod);
        var call = game.ImportReference(externalCall);
        
        injection.Apply(weaveMethod, targetMethod, call);
        Console.WriteLine($"Applied {injection.GetType().Name} from {weaveMethod.Name} to {targetMethod.Name}");
    }
    
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