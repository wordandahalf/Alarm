namespace Alarm.Mods;

public record ModConfiguration(
    string Name,
    string Author,
    string Version,
    string Entrypoint,
    string[] Weaves
);