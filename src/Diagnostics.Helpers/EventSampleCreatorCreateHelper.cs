using Microsoft.Diagnostics.Monitoring.EventPipe;
using System;

namespace Diagnostics.Helpers
{
    public static class EventSampleCreatorCreateHelper
    {
        public static ISampleProvider GetSample(this IEventSampleCreator creator, int processId, Action<IEventPipeProviderBuilder>? builderConfig = null, CounterConfiguration? configuration = null, bool requestRundown = true, int bufferSizeInMB = 256, int defaultIntervalSeconds = 1)
        {
            var counterProvider = creator.CreateCounterProvider();
            var counter = CounterHelper.CreateCounter(processId, creator.GetProviders(builderConfig), configuration, requestRundown, bufferSizeInMB);
            return creator.GetSample(counter);
        }
        public static ISampleProvider GetIntervalSample(this IEventSampleCreator creator, int processId,TimeSpan interval, Action<IEventPipeProviderBuilder>? builderConfig = null, CounterConfiguration? configuration = null, bool requestRundown = true, int bufferSizeInMB = 256, int defaultIntervalSeconds = 1)
        {
            var counterProvider = creator.CreateIntervalCounterProvider(interval);
            var counter = CounterHelper.CreateCounter(processId, creator.GetProviders(builderConfig), configuration, requestRundown, bufferSizeInMB);
            return creator.GetSample(counter);
        }
    }
}
