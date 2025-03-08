using System;

namespace API.Weaves
{
    public class TransformerAttribute : Attribute
    {
        public readonly int? Priority;
        
        protected TransformerAttribute(int? priority) =>
            Priority = priority < 0 ? null : priority;

        protected TransformerAttribute() : this(null) {}
    }
}