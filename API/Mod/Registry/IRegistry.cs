using System;
using System.Collections;
using System.Collections.Generic;

namespace API.Mod.Registry
{
    public interface IRegistry
    {
        public int Size { get; }
        public IEnumerable<NamespacedKey> Keys { get; }
        public IEnumerable Values { get; }
        
        public bool Contains(NamespacedKey key);
        public object? Get(NamespacedKey key);
    }
    
    // A key-value store.
    public interface IRegistry<T> : IRegistry where T : class
    {
        public new IEnumerable<NamespacedKey> Keys { get; }
        public new IEnumerable<T> Values { get; }
        
        public new T? Get(NamespacedKey key);
        public T? this[NamespacedKey key] => Get(key);

        public TK? Get<TK>(ResourceIdentifier<TK> key) where TK : class, T
        {
            NamespacedKey namespaced = key;
            return Get(namespaced) as TK ?? throw new InvalidOperationException("Key '" + namespaced + "' exists in map but with different type.");
        }
    }
}