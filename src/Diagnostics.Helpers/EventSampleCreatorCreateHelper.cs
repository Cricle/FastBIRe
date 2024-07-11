using System;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers
{
    public static class EventSampleCreatorCreateHelper
    {
        public static ISampleProvider GetSample(this IEventSampleCreator creator, int processId, Action<IEventPipeProviderBuilder>? builderConfig = null, CounterConfiguration? configuration = null, bool requestRundown = true, int bufferSizeInMB = 256, int defaultIntervalSeconds = 1)
        {
            var counter = CounterHelper.CreateCounter(processId, creator.GetProviders(builderConfig), configuration, requestRundown, bufferSizeInMB,defaultIntervalSeconds:defaultIntervalSeconds);
            return creator.GetSample(counter);
        }
        public static ISampleProvider GetIntervalSample(this IEventSampleCreator creator, int processId,TimeSpan interval, Action<IEventPipeProviderBuilder>? builderConfig = null, CounterConfiguration? configuration = null, bool requestRundown = true, int bufferSizeInMB = 256, int defaultIntervalSeconds = 1)
        {
            var counter = CounterHelper.CreateCounter(processId, creator.GetProviders(builderConfig), configuration, requestRundown, bufferSizeInMB,defaultIntervalSeconds:defaultIntervalSeconds);
            return creator.GetIntervalSample(counter, interval);
        }

        public static ISampleProvider GetSample(this IEventSampleCreator creator,Func<EventSource, bool>? isAccept)
        {
            var counter = CounterHelper.CreateCounter(isAccept);
            return creator.GetSample(counter);
        }

        public static ISampleProvider GetIntervalSample(this IEventSampleCreator creator, TimeSpan interval)
        {
            var counter = CounterHelper.CreateCounter(e => creator.IsAcceptProvider(e.Name),(int)interval.TotalSeconds);
            return creator.GetIntervalSample(counter, interval);
        }
    }
}
