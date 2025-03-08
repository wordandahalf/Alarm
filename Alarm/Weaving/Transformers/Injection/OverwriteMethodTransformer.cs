using System.Numerics;
using Alarm.Weaving.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Alarm.Weaving.Transformers.Injection;

public class OverwriteMethodTransformer(
    int? priority, MethodDefinition source, MethodDefinition target
) : BinaryMethodTransformer(ApplyPhase.Override, priority, source, target)
{
    public override void Apply()
    {
        var replacement = new MethodBody(
            new MethodDefinition(TargetMethod.Name, TargetMethod.Attributes, TargetMethod.ReturnType)
        );

        foreach (var i in SourceMethod.Body.Instructions)
        {
            replacement.Instructions.Add(i.UpdateReference(TargetMethod));
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