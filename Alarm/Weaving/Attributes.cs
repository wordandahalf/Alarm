using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Alarm.Weaving;

/// <summary>
/// Decorates a weave class
/// </summary>
/// <param name="name">The fully qualified name of the target type</param>
[AttributeUsage(AttributeTargets.Class)]
public class Weave(string name) : Attribute
{
    public string Name => name;
}

public interface IInjectionMethod
{
    public void Apply(MethodDefinition weave, MethodDefinition target, MethodReference call);
}

/// <summary>
/// Decorates an injection method
/// </summary>
/// <param name="at">The location where this method's bytecode is injected</param>
/// <param name="name">The name of the target method</param>
[AttributeUsage(AttributeTargets.Method)]
public class Inject(Inject.Location at, string? name = null) : Attribute, IInjectionMethod
{
    public Location At => at;
    public string? Name => name;
    
    public enum Location
    {
        Head, Tail
    }
    
    public void Apply(MethodDefinition weave, MethodDefinition target, MethodReference call)
    {
        var instr = target.Body.Instructions;
        var editor = target.Body.GetILProcessor();
        
        var insertion = at switch
        {
            Location.Head => instr.First(),
            Location.Tail => instr.Reverse().First(it => it.OpCode == OpCodes.Ret),
            _ => throw new ArgumentException($"unknown location '{at}'")
        };
        
        editor.InsertBefore(insertion, Instruction.Create(OpCodes.Call, call));
    }
}

/// <summary>
/// Decorates an overwrite method
/// </summary>
/// <param name="name">The name of the target method</param>
[AttributeUsage(AttributeTargets.Method)]
public class Overwrite(string? name = null) : Attribute, IInjectionMethod
{
    public string? Name => name;
    
    public void Apply(MethodDefinition weave, MethodDefinition target, MethodReference call)
    {
        var editor = target.Body.GetILProcessor();
        
        editor.Clear();
        editor.Append(Instruction.Create(OpCodes.Call, call));
        editor.Append(Instruction.Create(OpCodes.Ret));
    }
}