using System.Text.Json;
using Alarm.Mods.Registry;
using Alarm.Weaving;
using Alarm.Weaving.Utils;
using API.Mod.Content;
using API.Mod.Content.Wardrobe;
using API.Mod.Registry;
using Microsoft.Extensions.Logging;

namespace Alarm.Mods.Loading;

/// <summary>
/// Internal container for mod loading logic.
/// </summary>
internal class ModLoader
{
    private const string ModExtension = ".dll";
    private const string ModConfigName = "alarm_mod.json";
    private const string GameAssembly = "/Managed/Assembly-CSharp.dll";
    private readonly FileInfo _originalGameAssembly = new("./Assembly-CSharp.dll");
    
    private readonly ILoggerFactory _factory = LoggerFactory.Create(builder => builder.AddConsole());
    
    private readonly List<LoadedMod> _loadedMods = []; 
    
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
    
    public void ModifyGame(DirectoryInfo gameDirectory)
    {
        AppDomain.CurrentDomain.AssemblyResolve += Assemblies.Resolve;
        
        var gameFile = new FileInfo(gameDirectory.FullName + GameAssembly);

        if (gameFile.Exists && !_originalGameAssembly.Exists)
        {
            File.Move(gameFile.FullName, _originalGameAssembly.FullName);
            Console.WriteLine($"Created backup of game library at '{_originalGameAssembly.FullName}'");
        }

        File.Copy(_originalGameAssembly.FullName, gameFile.FullName, true);
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

    public void LoadMods()
    {
        var registries = new DictionaryRegistryAccess();
        registries.Set(Registries.Costumes, new DictionaryMutableRegistry<Costume>());
        
        foreach (var loadedMod in _loadedMods)
        {
            loadedMod.Implementation.OnLoad(registries);
        }
    }

    private LoadedMod LoadFile(FileInfo modFile)
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
        var modInstance = Activator.CreateInstance(entrypoint) as API.Mod.AlarmMod ?? throw new BadEntrypointException(modFile);
        modInstance.Initialize(_factory.CreateLogger(config.Name));
        
        return new LoadedMod(config, modInstance, mod);
    }
}