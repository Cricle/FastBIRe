using System;

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class CounterItemAttribute : Attribute
    {
        public CounterItemAttribute(string eventName)
        {
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
        }

        public string EventName { get; }
    }
}
