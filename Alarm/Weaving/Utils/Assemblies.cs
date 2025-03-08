using System.Reflection;

namespace Alarm.Weaving.Utils;

using Mono.Cecil;

internal static class Assemblies
{
    private static readonly DefaultAssemblyResolver Resolver = new();
    private static AssemblyDefinition? _gameAssembly;
    
    public static Assembly GetExecutingAssembly() => Assembly.GetExecutingAssembly();

    public static AssemblyDefinition GetGameAssembly()
    {
        return _gameAssembly
            ?? throw new InvalidOperationException("Cannot access game assembly before it is loaded");
    }

    public static void LoadGameAssembly(string fileName)
    {
        if (_gameAssembly != null) throw new InvalidOperationException();
        _gameAssembly = Load(fileName, readWrite: true);
    }

    public static void SaveGameAssembly()
    {
        if (_gameAssembly == null) throw new InvalidOperationException();
        _gameAssembly.Write();
    }
    
    /// <param name="fileName">The path to the assembly to load</param>
    /// <param name="readWrite">Whether the assembly should be loaded with write compatibility</param>
    /// <returns>The loaded assembly, cached if previously loaded</returns>
    public static AssemblyDefinition Load(string fileName, bool readWrite = false)
    {
        return AssemblyDefinition.ReadAssembly(
            fileName,
            new ReaderParameters { AssemblyResolver = Resolver, ReadWrite = readWrite }
        );
    }

    public static Assembly LoadRuntime(string fileName)
    {
        return Assembly.LoadFile(fileName);
    }

    public static void AddSearchDirectory(string directory)
    {
        Resolver.AddSearchDirectory(directory);
    }

    internal static Assembly? Resolve(object? sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name).Name + ".dll";
        var location = Resolver.GetSearchDirectories()
            .Select(directory => new FileInfo(directory + "/" + name))
            .FirstOrDefault(it => it.Exists);

        if (location is null) return null;
        Console.WriteLine($"Loading assembly '{name}' from '{location.FullName}'...");
        return LoadRuntime(location.FullName);
    }
}