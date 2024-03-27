using Microsoft.Diagnostics.NETCore.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnostics.Helpers
{
    public class EventSampleCreatorGroup : List<IEventSampleCreator>, IEventSampleCreator
    {
        public EventSampleCreatorGroup()
        {
        }

        public EventSampleCreatorGroup(IEnumerable<IEventSampleCreator> collection) : base(collection)
        {
        }

        public IEnumerable<string> ProviderNames => this.SelectMany(x => x.ProviderNames).Distinct();

        public bool SupportIntervalCounterProvider => this.All(x=>x.SupportIntervalCounterProvider);

        public IEventCounterProvider CreateCounterProvider()
        {
            return new EventCounterGroup(this.Select(x => x.CreateCounterProvider()));
        }

        public ISampleProvider GetSample(ICounterResult counterResult)
        {
            return new SampleResult<EventCounterGroup>(counterResult, (EventCounterGroup)CreateCounterProvider());
        }

        public IEnumerable<IEventSampleCreator> GetAcceptProvider(string name)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].IsAcceptProvider(name))
                {
                   yield return this[i];
                }
            }
        }

        public bool IsAcceptProvider(string name)
        {
            return GetAcceptProvider(name).Any();
        }

        public IEnumerable<EventPipeProvider> GetProviders(Action<IEventPipeProviderBuilder>? builderConfig = null)
        {
            for (int i = 0; i < Count; i++)
            {
                var provider = this[i];
                foreach (var item in provider.GetProviders(builderConfig))
                {
                    yield return item;
                }
            }
        }

        public IEventCounterProvider CreateIntervalCounterProvider(TimeSpan interval)
        {
            return new EventCounterGroup(this.Select(x => x.CreateIntervalCounterProvider(interval)));
        }

        public ISampleProvider GetIntervalSample(ICounterResult counterResult, TimeSpan interval)
        {
            return new SampleResult<EventCounterGroup>(counterResult, (EventCounterGroup)CreateIntervalCounterProvider(interval));
        }
    }
}
