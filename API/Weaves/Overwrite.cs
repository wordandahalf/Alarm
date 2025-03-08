using System;

namespace API.Weaves
{
    /// <summary>
    /// Decorates an overwrite method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Overwrite : TransformerAttribute
    {
        public Overwrite(int priority = -1) : base(priority) { }
    }
}