using System;

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ActivityMapToEventSourceAttribute : Attribute
    {
        public ActivityMapToEventSourceAttribute(Type eventType, int mappedCount)
        {
            EventType = eventType;
            MappedCount = mappedCount;
        }

        public Type EventType { get; }

        public int MappedCount { get; }
    }
}
