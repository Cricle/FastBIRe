using System;

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class MapToEventSourceGenerateIdIgnoreAttribute : Attribute
    {

    }
}
