using System;

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MapToActivityAttribute : Attribute
    {
        public MapToActivityAttribute(Type activityClassType)
        {
            ActivityClassType = activityClassType ?? throw new ArgumentNullException(nameof(activityClassType));
        }

        public Type ActivityClassType { get; }

        public bool WithEventSourceCall { get; set; }

        public bool CallEventAtEnd { get; set; }
    }
}
