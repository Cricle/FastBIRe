using System;

namespace Diagnostics.Generator.Core.Annotations
{
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
