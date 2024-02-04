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
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ActivityMapToEventAttribute : Attribute
    {
        public ActivityMapToEventAttribute(int eventId, string methodName, Type[] paramterTypes)
        {
            EventId = eventId;
            MethodName = methodName;
            ParamterTypes = paramterTypes;
        }

        public int EventId { get; }

        public string MethodName { get; }

        public Type[] ParamterTypes { get; }
    }
}
