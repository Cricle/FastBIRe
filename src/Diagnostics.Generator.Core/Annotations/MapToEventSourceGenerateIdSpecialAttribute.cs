using System;

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class MapToEventSourceGenerateIdSpecialAttribute : Attribute
    {
        public MapToEventSourceGenerateIdSpecialAttribute(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}
