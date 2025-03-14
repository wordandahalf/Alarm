using System.Diagnostics;
using System.Runtime.InteropServices;
using Alarm.Mods.Loading;
using Alarm.Weaving.Utils;

[DllImport(@"C:\Program Files (x86)\Steam\steamapps\common\WakeyWakey\UnityPlayer.dll")]
static extern int UnityMain(IntPtr hInstance, IntPtr hPrevInstance, [MarshalAs(UnmanagedType.LPWStr)] string lpArgs, int nShowCmd);

Assemblies.AddSearchDirectory(@"C:\Program Files (x86)\Steam\steamapps\common\WakeyWakey\Wakey Wakey_Data\Managed");
var loader = new ModLoader();

loader.LoadDirectory(new DirectoryInfo(Directory.GetCurrentDirectory()));

var mods = loader.LoadedMods;

loader.ModifyGame(new DirectoryInfo(@"C:\Program Files (x86)\Steam\steamapps\common\WakeyWakey\Wakey Wakey_Data"));
loader.LoadMods();

Console.WriteLine($"Loaded {mods.Length} mods: {string.Join(", ", mods.Select(it => $"{it.Config.Name} v{it.Config.Version}"))}");

// UnityMain(Process.GetCurrentProcess().Handle, IntPtr.Zero, "-screen-fullscreen", 1);