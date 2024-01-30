using System;

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CounterMappingAttribute : Attribute
    {
        public bool ForAnysProviders { get; set; }

        public string[] ForProviders { get; set; }
    }
}
