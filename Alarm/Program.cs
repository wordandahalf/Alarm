using Alarm.Mod.Loading;

var mods =
    ModLoader.LoadDirectory(new DirectoryInfo("/home/issl/Documents/Workspace/Rider/Alarm/TestMod/bin/Debug/net9.0"));

Console.WriteLine($"Loaded {mods.Length} mods: {string.Join(", ", mods.Select(it => $"{it.Config.Name} v{it.Config.Version}"))}");

foreach (var loadedMod in mods)
{
    loadedMod.Implementation.Initialize();
}