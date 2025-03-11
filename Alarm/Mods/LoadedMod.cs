using System.Reflection;

namespace Alarm.Mods;

public record LoadedMod(ModConfiguration Config, API.Mod.AlarmMod Implementation, Assembly Assembly);