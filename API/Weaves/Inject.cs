using System;

namespace API.Weaves
{
    /// <summary>
    /// Decorates an injection method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Inject : TransformerAttribute
    {
        public enum Location
        {
            Head, Tail
        }
        
        public readonly Location At;
        public readonly string? Name;
        
        /// <param name="at">The location where this method's bytecode is injected</param>
        /// <param name="name">The name of the target method</param>
        public Inject(Location at, string? name = null, int priority = -1) : base(priority)
        {
            At = at;
            Name = name;
        }
    }
}