namespace Alarm.Mod.Loading;

public class ModLoadingException(FileInfo mod, string? message)
    : Exception($"Could not load mod '{mod.FullName}': {message}");
public class MissingConfigurationException(FileInfo mod) : ModLoadingException(mod, "no mod configuration was found");
public class TooManyConfigurationsException(FileInfo mod) : ModLoadingException(mod, "too many configurations were found");
public class IllegalConfigurationException(FileInfo mod) : ModLoadingException(mod, "malformed configuration");
public class MissingEntrypointException(FileInfo mod, string entrypoint) :
    ModLoadingException(mod, $"missing entrypoint '{entrypoint}'");
public class BadEntrypointException(FileInfo mod)
    : ModLoadingException(mod, "specified entrypoint is not the correct subclass");