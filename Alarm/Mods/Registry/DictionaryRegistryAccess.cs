using System.Collections;
using API.Mod.Registry;

namespace Alarm.Mods.Registry;

public class DictionaryRegistryAccess : IRegistryAccess, IMutableRegistry
{
    private readonly IMutableRegistry _registries = new DictionaryMutableRegistry<IRegistry>();
    IEnumerable IRegistry.Values => _registries.Values;

    public object? Get(NamespacedKey key) => _registries.Get(key);

    public IEnumerable<NamespacedKey> Keys => _registries.Keys;
    public int Size => _registries.Size;

    public bool Contains(NamespacedKey key) => _registries.Contains(key);

    private TK Get<TK>(ResourceIdentifier<TK> key) where TK : class
    {
        NamespacedKey namespaced = key;
        return _registries.Get(namespaced) as TK ?? throw new InvalidOperationException("Key '" + namespaced + "' exists in map but with different type.");
    }
    
    public IMutableRegistry<T> Get<T>(ResourceIdentifier<IMutableRegistry<T>> registry) where T : class =>
        Get<IMutableRegistry<T>>(registry);

    public IRegistry<T> Get<T>(ResourceIdentifier<IRegistry<T>> registry) where T : class =>
        Get<IRegistry<T>>(registry);

    public void Remove(NamespacedKey key) => _registries.Remove(key);
    public void Set(NamespacedKey key, object value) => _registries.Set(key, value);
    public void Freeze() => _registries.Freeze();
    public bool Frozen() => _registries.Frozen();
}