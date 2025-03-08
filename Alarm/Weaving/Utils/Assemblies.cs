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
        // todo: Resolver.AddSearchDirectory();
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
}