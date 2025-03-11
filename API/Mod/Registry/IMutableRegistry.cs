using System;

namespace API.Mod.Registry
{
    public interface IMutableRegistry : IRegistry
    {
        public void Remove(NamespacedKey key);
        public void Set(NamespacedKey key, object value);
        public void Freeze();
        public bool Frozen();
    }
    
    /// <summary>
    /// A key-value store with controllable mutability. Entries may be freely modified
    /// until the registry is marked as "frozen". 
    /// </summary>
    public interface IMutableRegistry<T> : IMutableRegistry, IRegistry<T> where T : class
    {
        public new T? this[NamespacedKey key]
        {
            get => Get(key);
            set => Set(key, value ?? throw new ArgumentNullException(nameof(value)));
        }

        void IMutableRegistry.Set(NamespacedKey key, object value)
        {
            var cast = value as T ??
                throw new InvalidOperationException($"Cannot insert value of type '{value.GetType().Name}' into a registry of '{typeof(T).Name}'");
            
            Set(key, cast);
        }
        
        public void Set<TK>(NamespacedKey key, TK value) where TK : T;
    }
}