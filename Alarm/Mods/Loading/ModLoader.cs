using System.Reflection;
using System.Text.Json;
using Alarm.Weaving;
using Alarm.Weaving.Utils;
using Mono.Cecil;

namespace Alarm.Mods.Loading;

/// <summary>
/// Internal container for mod loading logic.
/// </summary>
internal class ModLoader
{
    private const string ModExtension = ".dll";
    private const string ModConfigName = "alarm_mod.json";
    private const string GameAssembly = "/Managed/Assembly-CSharp.dll";
    
    private readonly List<LoadedMod> _loadedMods = new(); 
    
    public LoadedMod[] LoadedMods => _loadedMods.ToArray();
    
    public void LoadDirectory(DirectoryInfo modDirectory)
    {
        _loadedMods.AddRange(
            modDirectory.GetFiles()
                .Where(it => it.Name.EndsWith(ModExtension))
                .Select(it => {
                    try { return LoadFile(it); }
                    catch (ModLoadingException e) {
                        Console.Error.WriteLine(e.Message);
                        return null;
                    }
                })
                .Where(it => it != null)
                .ToArray() as LoadedMod[]
            );
    }
    
    public void Initialize(DirectoryInfo gameDirectory)
    {
        var gameFile = new FileInfo(gameDirectory.FullName + GameAssembly);
        var backupFile = new FileInfo(gameFile.FullName + ".bak");

        if (gameFile.Exists && !backupFile.Exists)
        {
            File.Move(gameFile.FullName, backupFile.FullName);
            Console.WriteLine($"Created backup of game library at '{backupFile.FullName}'");
        }

        File.Copy(backupFile.FullName, gameFile.FullName, true);
        Assemblies.LoadGameAssembly(gameFile.FullName);
        
        foreach (var (config, _, assembly) in _loadedMods)
        {
            foreach (var weave in config.Weaves)
            {
                Weaves.Load(
                    assembly.GetType(weave)
                    ?? throw new MissingWeaveException(assembly, weave)
                );
            }
        }

        Weaves.Apply();
        Assemblies.SaveGameAssembly();
    }

    private static LoadedMod LoadFile(FileInfo modFile)
    {
        var mod =
            modFile.FullName == Assemblies.GetExecutingAssembly().Location ?
                Assemblies.GetExecutingAssembly() : Assemblies.LoadRuntime(modFile.FullName);
        
        var configName = mod.GetManifestResourceNames().FirstOrDefault(it => it.EndsWith(ModConfigName));
        if (configName == null) throw new MissingConfigurationException(modFile);
        
        using var stream = mod.GetManifestResourceStream(configName) ?? throw new IllegalConfigurationException(modFile);
        var config = JsonSerializer.Deserialize<ModConfiguration>(stream)
                                  ?? throw new IllegalConfigurationException(modFile);

        var entrypoint = mod.GetType(config.Entrypoint) ?? throw new MissingEntrypointException(modFile, config.Entrypoint);
        var modInstance = Activator.CreateInstance(entrypoint) as IMod ?? throw new BadEntrypointException(modFile);
        
        return new LoadedMod(config, modInstance, mod);
    }
}