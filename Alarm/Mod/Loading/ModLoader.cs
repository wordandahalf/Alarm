using System.Reflection;
using System.Text.Json;
using Alarm.Weaving;
using Mono.Cecil;

namespace Alarm.Mod.Loading;

/// <summary>
/// Internal container for mod loading logic.
/// </summary>
internal class ModLoader
{
    private const string ModExtension = ".dll";
    private const string ModConfigName = "alarm_mod.json";
    private const string GameAssembly = "/Managed/Assembly-CSharp.dll";
    
    private readonly List<LoadedMod> _loadedMods = new List<LoadedMod>(); 
    
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
        Hook(backupFile);
    }

    private void Hook(FileInfo gameFile)
    {
        var assembly = AssemblyDefinition.ReadAssembly(gameFile.FullName).MainModule;

        // todo: error handling
        foreach (var mod in _loadedMods)
        {
            foreach (var weave in mod.Config.Weaves)
            {
                Console.WriteLine($"Handling configured weave '{weave}' for mod '{mod.Config.Name}");
                Weaving.Weaving.HandleClass(
                    assembly,
                    AssemblyDefinition.ReadAssembly(mod.Assembly.Location).MainModule, 
                    mod.Assembly.GetType(weave)!
                );
            }
        }
        
        assembly.Write(gameFile.FullName.Replace(".bak", null));
        Console.WriteLine($"Wrote modified code to '{gameFile.FullName.Replace(".bak", null)}'");
    }

    private static LoadedMod LoadFile(FileInfo modFile)
    {
        var mod = Assembly.LoadFile(modFile.FullName);
        var configNames = mod.GetManifestResourceNames().Where(it => it.EndsWith(ModConfigName)).ToArray();

        if (configNames.Length == 0) throw new MissingConfigurationException(modFile);
        if (configNames.Length > 1) throw new TooManyConfigurationsException(modFile);
        
        var configName = configNames[0];
        
        using var stream = mod.GetManifestResourceStream(configName) ?? throw new IllegalConfigurationException(modFile);
        var config = JsonSerializer.Deserialize<ModConfiguration>(stream)
                                  ?? throw new IllegalConfigurationException(modFile);

        var entrypoint = mod.GetType(config.Entrypoint) ?? throw new MissingEntrypointException(modFile, config.Entrypoint);
        var modInstance = Activator.CreateInstance(entrypoint) as IMod ?? throw new BadEntrypointException(modFile);
        
        return new LoadedMod(config, modInstance, mod);
    }
}