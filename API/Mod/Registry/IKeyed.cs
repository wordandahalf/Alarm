namespace API.Mod.Registry
{
    /// <summary>
    /// An object with an associated NamespacedKey.
    /// </summary>
    public interface IKeyed
    {
        public NamespacedKey Key { get; }
    }
}