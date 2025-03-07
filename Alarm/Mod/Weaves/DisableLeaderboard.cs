using Alarm.Weaving;

namespace Alarm.Mod.Weaves;

[Weave("DatabaseManager")]
public class DisableLeaderboard
{
    [Overwrite]
    public static void WriteToLeaderboard() {}
}