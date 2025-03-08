using System;

namespace API.Weaves
{
    /// <summary>
    /// Decorates a weave class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class Weave : Attribute
    {
        public readonly string Type;
        
        /// <param name="type">The fully qualified name of the target type</param>
        public Weave(string type) => Type = type;
    }
}