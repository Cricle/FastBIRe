using System;
#if NET8_0_OR_GREATER
#endif

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public sealed class ActivityIgnoreAttribute : Attribute
    {
    }
}
