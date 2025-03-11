using API.Mod.Content.Wardrobe;
using API.Mod.Registry;

namespace API.Mod.Content
{
    public static class Registries
    {
        public static readonly ResourceIdentifier<IMutableRegistry<Costume>> Costumes = ResourceIdentifier<IMutableRegistry<Costume>>.Of("costumes");
    }
}