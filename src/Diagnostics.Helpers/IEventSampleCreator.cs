using Microsoft.Diagnostics.NETCore.Client;
using System;
using System.Collections.Generic;

namespace Diagnostics.Helpers
{
    public interface IEventSampleCreator : IEventCounterProviderCreator, ISampleProviderCreator
    {
        IEnumerable<EventPipeProvider> GetProviders(Action<IEventPipeProviderBuilder>? builderConfig = null);

        IEnumerable<string> ProviderNames { get; }

        bool IsAcceptProvider(string name);
    }
}
