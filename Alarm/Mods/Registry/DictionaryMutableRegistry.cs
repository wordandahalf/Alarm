using System.Collections;
using API.Mod.Registry;

namespace Alarm.Mods.Registry;

public class DictionaryMutableRegistry<T> : IMutableRegistry<T> where T : class
{
    private readonly Dictionary<NamespacedKey, T> _registry = new();
    private bool _frozen;

    public int Size => _registry.Count;
    public IEnumerable<NamespacedKey> Keys => _registry.Keys;
    IEnumerable IRegistry.Values => Values;
    public IEnumerable<T> Values => _registry.Values;

    public bool Contains(NamespacedKey key) => _registry.ContainsKey(key);

    object IRegistry.Get(NamespacedKey key) => Get(key);
    public T Get(NamespacedKey key) => _registry[key];

    public void Remove(NamespacedKey key) => _registry.Remove(key);
    
    public void Set<TK>(NamespacedKey key, TK value) where TK : T
    {
        if (_frozen) throw new InvalidCastException("Cannot modify frozen registry.");
        _registry[key] = value;
    }

    public void Freeze() => _frozen = true;
    public bool Frozen() => _frozen;
}