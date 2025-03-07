using System.Reflection;
using Alarm.Mods;
using Alarm.Weaving.Transformers;
using Mono.Cecil;

namespace Alarm.Weaving;

public static class Weaves
{
    private static readonly string WeaveTypeName = typeof(Weave).FullName!;
    
    private static readonly IDictionary<string, WeaveTarget> LoadedWeaves = new Dictionary<string, WeaveTarget>();
    
    /// <summary>
    /// Parses the Weave attributes attached to the provided type, returning an assembled target.
    /// If the target of the weave has already been modified, this method will properly combine their changes.
    /// </summary>
    public static void Load(Type weave)
    {
        Console.WriteLine($"Loading weave '{weave.FullName}'");
        
        // todo: error checking
        var source = GetOrCreateTarget(weave.FullName!, Assemblies.Load(weave.Assembly.Location))!;
        var target = GetOrCreateTarget(((Weave) weave.GetCustomAttribute(typeof(Weave))!).Type)!;
        
        foreach (var method in weave.GetRuntimeMethods())
        {
            HandleMethod(source, target, method);
        }

        foreach (var field in weave.GetRuntimeFields())
        {
            HandleField(source, target, field);
        }
    }

    // todo: error handling
    private static void HandleMethod(WeaveTarget source, WeaveTarget target, MethodInfo info)
    {
        foreach (TransformerAttribute attr in info.GetCustomAttributes(typeof(TransformerAttribute)))
        {
            attr.AddTransformers(source, target, source.Definition.GetMethod(info)!);
        }
    }

    // todo: error handling
    private static void HandleField(WeaveTarget source, WeaveTarget target, FieldInfo info)
    {
        foreach (TransformerAttribute attr in info.GetCustomAttributes(typeof(TransformerAttribute)))
        {
            attr.AddTransformers(source, target, source.Definition.GetField(info)!);
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
        if (LoadedWeaves.ContainsKey(typeName))
        {
            return LoadedWeaves[typeName];
        }
        else
        {
            var target = new WeaveTarget(assembly, assembly.MainModule.GetType(typeName));
            LoadedWeaves[typeName] = target;

            return target;
        }
    }
}