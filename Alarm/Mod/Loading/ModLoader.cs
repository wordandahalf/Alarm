using System.Reflection;
using System.Text.Json;

namespace Alarm.Mod.Loading;

/// <summary>
/// Internal container for mod loading logic.
/// </summary>
internal static class ModLoader
{
    private const string ModExtension = ".dll";
    private const string ModConfigName = "alarm_mod.json";
    
    public static LoadedMod[] LoadDirectory(DirectoryInfo modDirectory)
    {
        return modDirectory.GetFiles()
            .Where(it => it.Name.EndsWith(ModExtension))
            .Select(it => {
                try { return LoadFile(it); }
                catch (ModLoadingException e) {
                    Console.Error.WriteLine(e.Message);
                    return null;
                }
            })
            .Where(it => it != null)
            .ToArray() as LoadedMod[];
    }

    public static LoadedMod LoadFile(FileInfo modFile)
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