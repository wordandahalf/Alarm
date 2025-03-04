namespace Alarm.Mod;

/// <summary>
/// Interface implemented by Alarm mods.
/// </summary>
public interface IMod
{
    /// <summary>
    /// Called at load-time for initialization.
    /// </summary>
    public void Initialize();
}