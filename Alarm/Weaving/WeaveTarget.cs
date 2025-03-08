using Alarm.Weaving.Transformers;
using Mono.Cecil;

namespace Alarm.Weaving;

public record WeaveTarget(AssemblyDefinition Assembly, TypeDefinition Definition)
{
    public readonly List<WeaveTransformer> Transformers = [];
    
    public void AddTransformers(params WeaveTransformer[] transformers)
    {
        Transformers.AddRange(transformers.ToList());
    }
}