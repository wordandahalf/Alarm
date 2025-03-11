using System.Collections.Generic;

namespace API.Mod.Registry
{
    /// <summary>
    /// Convenience typealias to reduce verbosity. See <see cref="AlarmMod.OnLoad"/>.
    /// </summary>
    public interface IRegistryAccess
    {
        public IEnumerable<NamespacedKey> Keys { get; }
        public int Size { get; }
        
        public IMutableRegistry<T> Get<T>(ResourceIdentifier<IMutableRegistry<T>> registry) where T : class;
        public IRegistry<T> Get<T>(ResourceIdentifier<IRegistry<T>> registry) where T : class;
    }
}