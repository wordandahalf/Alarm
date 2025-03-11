using System;
using System.Linq;

namespace API.Mod.Registry
{
    /// <summary>
    /// An identifier with type information.
    /// </summary>
    public class ResourceIdentifier<T> : NamespacedKey
    {
        public readonly Type Type = typeof(T);
        ResourceIdentifier(string @namespace, string key) : base(@namespace, key) {}

        public static ResourceIdentifier<T> Of(string @namespace, string key)
        {
            return new ResourceIdentifier<T>(@namespace, key);
        }
        
        public static ResourceIdentifier<T> Of(string key)
        {
            return new ResourceIdentifier<T>(WakeyNamespace, key);
        }

        public override string ToString() =>
            Type.GetGenericArguments().Length > 0
                ? $"{Namespace}.{Key} [{Type.Name}<{string.Join(", ", Type.GetGenericArguments().Select(it => it.Name))}>]"
                : $"{Namespace}.{Key} [{Type.Name}]";
        
        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;
            
            var that = (ResourceIdentifier<T>)other;
            return base.Equals(that) && Type == that.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Type);
        }
    }
}