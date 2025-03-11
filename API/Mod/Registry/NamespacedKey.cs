using System;

namespace API.Mod.Registry
{
    /// <summary>
    /// An identifier. The namespace indicates the "source" of the identified resource,
    /// while the key serves as the unique identifier. 
    /// </summary>
    public class NamespacedKey
    {
        public const string WakeyNamespace = "wakey";
        
        public string Namespace { get; }
        public string Key { get; }

        public NamespacedKey(string @namespace, string key)
        {
            Namespace = @namespace;
            Key = key;
        }

        public override string ToString() => $"{Namespace}:{Key}";

        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;
            
            var key = (NamespacedKey)other;
            return key.Key != Key || key.Namespace != Namespace;
        }
        
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Namespace, StringComparer.OrdinalIgnoreCase);
            hashCode.Add(Key, StringComparer.OrdinalIgnoreCase);
            return hashCode.ToHashCode();
        }
    }
}