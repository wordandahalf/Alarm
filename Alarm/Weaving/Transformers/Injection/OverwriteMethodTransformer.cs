using System.Numerics;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Alarm.Weaving.Transformers.Injection;

public class OverwriteMethodTransformer(
    int? priority, MethodDefinition source, MethodDefinition target
) : BinaryMethodTransformer(ApplyPhase.Override, priority, source, target)
{
    public override void Apply()
    {
        Console.WriteLine($"Overwriting target {TargetMethod.FullName} with {SourceMethod.FullName}");
        
        var replacement = new MethodBody(
            new MethodDefinition(TargetMethod.Name, TargetMethod.Attributes, TargetMethod.ReturnType)
        );

        var processor = SourceMethod.Body.GetILProcessor();

        Console.WriteLine($"Target module: {TargetMethod.Module.FileName}");
        foreach (var i in SourceMethod.Body.Instructions)
        {
            // todo: common method
            replacement.Instructions.Add(
                i.Operand switch
                {
                    MethodReference methodReference => 
                        processor.Create(i.OpCode, TargetMethod.Module.ImportReference(methodReference)),
                    TypeReference typeReference =>
                        processor.Create(i.OpCode, TargetMethod.Module.ImportReference(typeReference)),
                    FieldReference fieldReference =>
                        processor.Create(i.OpCode, TargetMethod.Module.ImportReference(fieldReference)),
                    _ => i
                }
            );
        }

        foreach (var v in SourceMethod.Body.Variables)
        {
            replacement.Variables.Add(
                new VariableDefinition(Target.Module.ImportReference(v.VariableType))
            );
        }
        
        TargetMethod.Body = replacement;
    }
}