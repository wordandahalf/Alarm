using API.Weaves;

namespace Mod.Weaves
{
    [Weave("DatabaseManager")]
    public class DisableLeaderboard
    {
        [Overwrite]
        public static void WriteToLeaderboard()
        {
        }
    }
}