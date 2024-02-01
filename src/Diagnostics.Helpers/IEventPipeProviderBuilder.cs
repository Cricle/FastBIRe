using Microsoft.Diagnostics.NETCore.Client;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers
{
    public interface IEventPipeProviderBuilder
    {
        string Name { get; }

        long Keywords { get; set; }

        EventLevel EventLevel { get; set; }

        IDictionary<string, string>? Arguments { get; set; }

        EventPipeProvider Build();
    }
}
