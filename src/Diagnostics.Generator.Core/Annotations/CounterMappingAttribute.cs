using System;

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CounterMappingAttribute : Attribute
    {
        public bool ForAnysProviders { get; set; }

        public string[] ForProviders { get; set; }

        public bool WithInterval { get; set; }

        public bool WithCreator { get; set; }

        public bool CreatorHasInstance { get; set; }
    }
}
