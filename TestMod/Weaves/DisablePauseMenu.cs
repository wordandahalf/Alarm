using Alarm.Weaving;

namespace TestMod.Weaves;

[Weave("InputManager")]
public class DisablePauseMenu
{
    [Overwrite]
    private static void OnMenuPause()
    {
        
    }
}