using System.Reflection;

namespace Alarm.Weaving;

using Mono.Cecil;

internal static class Assemblies
{
    private static readonly DefaultAssemblyResolver Resolver = new();
    private static readonly AssemblyDefinition ExecutingAssembly = Load(Assembly.GetExecutingAssembly().Location);
    private static AssemblyDefinition? _gameAssembly;
    
    public static Assembly GetExecutingAssembly() => Assembly.GetExecutingAssembly();
    // todo: error checking
    public static AssemblyDefinition GetGameAssembly() { return _gameAssembly!; }

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

    // todo: error checking
    /// <summary>
    /// Convenience method for loading an assembly and finding a type
    /// </summary>
    /// <returns>The provided type</returns>
    public static TypeDefinition LoadAndGetType(string fileName, string typeName)
    {
        return Load(fileName).MainModule.GetType(typeName);
    }
    
    /// <param name="fileName">The path to the assembly to load</param>
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