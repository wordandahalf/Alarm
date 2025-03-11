using System;
using API.Mod.Registry;
using Microsoft.Extensions.Logging;

namespace API.Mod
{
    /// <summary>
    /// Interface implemented by Alarm mods.
    /// </summary>
    public abstract class AlarmMod
    {
        protected ILogger Logger { get; private set; }

        public bool Initialized { get; private set; }

        public void Initialize(ILogger logger)
        {
            if (Initialized) throw new InvalidOperationException("Cannot initialize an already-initialized mod.");

            Logger = logger;
            Initialized = true;
        }
        
        /// <summary>
        /// Called at load-time for initialization.
        /// </summary>
        /// <param name="registryAccess">A reference to game registries, see <see cref="API.Mod.Content.Registries"/>.</param>
        public abstract void OnLoad(IRegistryAccess registryAccess);
    }   
}