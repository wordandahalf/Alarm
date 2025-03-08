using API.Weaves;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mod.Weaves
{
    [Weave("LevelManager")]
    public class AddModWatermark
    {
        public static Color Darken(Color color, float amount)
        {
            var ratio = 1 - amount;
            return new Color(color.r * ratio, color.g * ratio, color.b * ratio, color.a);
        }

        [Inject(Inject.Location.Tail)]
        public void Start()
        {
            var behavior = Object.FindFirstObjectByType<LevelObjectBehavior>();
            if (behavior != null)
            {
                behavior.levelName.color = Darken(behavior.levelName.color, 0.25f);
                behavior.levelNumber.color = Darken(behavior.levelNumber.color, 0.25f);
                behavior.darkenEverything.color = Darken(behavior.darkenEverything.color, 0.25f);
            }
        }
    }
}