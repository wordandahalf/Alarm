using System.Reflection;
using Alarm.Mods;
using Alarm.Weaving.Transformers;
using Alarm.Weaving.Utils;
using Mono.Cecil;

namespace Alarm.Weaving;

public static class Weaves
{
    private static readonly string WeaveTypeName = typeof(Weave).FullName!;
    
    private static readonly Dictionary<string, WeaveTarget> LoadedWeaves = new();
    
    /// <summary>
    /// Parses the Weave attributes attached to the provided type, returning an assembled target.
    /// If the target of the weave has already been modified, this method will properly combine their changes.
    /// </summary>
    public static void Load(Type weave)
    {
        var source =
            GetOrCreateTarget(
                weave.FullName ?? throw new BadWeaveException(weave), 
            Assemblies.Load(weave.Assembly.Location)
            ) ?? throw new BadWeaveException(weave);

        var weaveTarget =
            weave.GetCustomAttribute<Weave>()?.Type ?? throw new MissingWeaveAttributeException(weave); 
        var target =
            GetOrCreateTarget(weaveTarget) ?? throw new BadWeaveTargetException(weave, weaveTarget);
        
        foreach (var method in weave.GetRuntimeMethods())
        {
            HandleMethod(source, target, method);
        }

        foreach (var field in weave.GetRuntimeFields())
        {
            HandleField(source, target, field);
        }
    }

    private static void HandleMethod(WeaveTarget source, WeaveTarget target, MethodInfo info)
    {
        foreach (var attr in info.GetCustomAttributes<TransformerAttribute>())
        {
            attr.AddTransformers(
                source, target, 
                source.Definition.GetMethod(info)
                    ?? throw new InvalidOperationException($"Could not find method decorated method '{info}' in type '{source.Definition.FullName}'")
            );
        }
    }

    private static void HandleField(WeaveTarget source, WeaveTarget target, FieldInfo info)
    {
        foreach (var attr in info.GetCustomAttributes<TransformerAttribute>())
        {
            attr.AddTransformers(
                source, target, 
                source.Definition.GetField(info)
                    ?? throw new InvalidOperationException($"Could not find method decorated field '{info}' in type '{source.Definition.FullName}'")
            );
        }
    }

    public static void Apply()
    {
        var ordered = LoadedWeaves.SelectMany(it => it.Value.Transformers)
            .OrderBy(it => it.Phase)
            .ThenBy(it => it.Priority ?? int.MaxValue);
        
        foreach (var transformer in ordered)
        {
            transformer.Apply();
        }
    }

    private static WeaveTarget? GetOrCreateTarget(string typeName)
    {
        return GetOrCreateTarget(typeName, Assemblies.GetGameAssembly());
    }
    
    private static WeaveTarget? GetOrCreateTarget(string typeName, AssemblyDefinition assembly)
    {
        if (LoadedWeaves.TryGetValue(typeName, out var weaveTarget))
            return weaveTarget;

        var target = new WeaveTarget(assembly, assembly.MainModule.GetType(typeName));
        LoadedWeaves[typeName] = target;

        return target;
    }
}