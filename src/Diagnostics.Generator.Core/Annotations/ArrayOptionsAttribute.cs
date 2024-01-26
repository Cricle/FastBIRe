using System;

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class ArrayOptionsAttribute : Attribute
    {
        public ArrayOptionsAttribute(ArrayOptions options)
        {
            Options = options;
        }

        public ArrayOptions Options { get; }
    }
}
