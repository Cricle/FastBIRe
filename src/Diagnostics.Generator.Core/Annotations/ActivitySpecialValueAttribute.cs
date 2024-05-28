using System;

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ActivitySpecialValueAttribute : Attribute
    {
        public bool FilePath { get; set; }

        public bool MemberName { get; set; }

        public bool LineNumber { get; set; }
    }
}
