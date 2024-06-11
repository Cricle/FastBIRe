using System;

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Struct, Inherited = false)]
    public sealed class ActivityAsAttribute : Attribute
    {
        public Type? TargetType { get; set; }

        public ActivityAsTypes As { get; set; }

        public string? Key { get; set; }

        public string[]? IgnorePaths { get; set; }

        public bool GenerateSingleton { get; set; }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    public sealed class ActivityAsNameAttribute : Attribute
    {
        public ActivityAsNameAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    public sealed class ActivityAsIgnoreAttribute : Attribute
    {
    }
}
