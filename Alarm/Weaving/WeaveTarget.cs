using Alarm.Weaving.Transformers;
using Mono.Cecil;

namespace Alarm.Weaving;

public record WeaveTarget(AssemblyDefinition Assembly, TypeDefinition Definition)
{
    // todo: priority of transformer
    public readonly List<WeaveTransformer> Transformers = new();
    
    public void AddTransformers(params WeaveTransformer[] transformers)
    {
        Transformers.AddRange(transformers.ToList());
    }
}