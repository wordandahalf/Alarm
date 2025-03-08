using System.Reflection;
using API.Mod;

namespace Alarm.Mods;

public record LoadedMod(ModConfiguration Config, IMod Implementation, Assembly Assembly);