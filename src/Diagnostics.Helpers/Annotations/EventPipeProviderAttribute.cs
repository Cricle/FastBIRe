using Microsoft.Diagnostics.NETCore.Client;
using System;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class EventPipeProviderAttribute : Attribute
    {
        public EventPipeProviderAttribute(string name, EventLevel level)
        {
            Name = name;
            Level = level;
        }

        public string Name { get; }

        public EventLevel Level { get; }

        public long Keywords { get; set; }

        /// <summary>
        /// The <see cref="EventPipeProvider.Arguments"/> inputs, parse by interval=123,a1=2 or "interval"="123","a1"="2"
        /// </summary>
        public string? Arguments { get; set; }
    }
}
