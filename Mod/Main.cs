using API.Mod;
using API.Mod.Registry;
using Microsoft.Extensions.Logging;

namespace Mod
{
    public class Main : AlarmMod
    {
        public override void OnLoad(IRegistryAccess registryAccess)
        {
            Logger.LogInformation($"Loaded with {registryAccess.Size} registries: {string.Join(", ", registryAccess.Keys)}");
        }
    }
}