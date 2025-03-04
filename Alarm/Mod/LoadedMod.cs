using System.Reflection;

namespace Alarm.Mod;

public record LoadedMod(ModConfiguration Config, IMod Implementation, Assembly Assembly);