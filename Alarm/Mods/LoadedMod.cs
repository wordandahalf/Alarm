using System.Reflection;
using Alarm.Weaving;
using Mono.Cecil;

namespace Alarm.Mods;

public record LoadedMod(ModConfiguration Config, IMod Implementation, Assembly Assembly);