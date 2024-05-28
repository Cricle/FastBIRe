using Microsoft.Diagnostics.NETCore.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers
{
    public class EventPipeProviderBuilder : IEventPipeProviderBuilder
    {
        public EventPipeProviderBuilder(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
#if NET8_0_OR_GREATER
                ArgumentException.ThrowIfNullOrEmpty(nameof(name));
#else
                throw new ArgumentException($"name can't be null");
#endif
            }

            Name = name;
            Keywords = 0xF00000000000;
            EventLevel = EventLevel.Informational;
        }

        public string Name { get; }

        public long Keywords { get; set; }

        public EventLevel EventLevel { get; set; }

        public IDictionary<string, string>? Arguments { get; set; }

        public EventPipeProvider Build()
        {
            return new EventPipeProvider(Name, EventLevel, Keywords, Arguments);
        }
    }
}
