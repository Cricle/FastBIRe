using Diagnostics.Generator.Core.Annotations;
using Diagnostics.Helpers.Annotations;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers
{
    [EventPipeProvider(WellKnowsEventProvider.NetHttp, EventLevel.Informational)]
    [CounterMapping(ForAnysProviders = true, ForProviders = new[] { WellKnowsEventProvider.NetHttp }, WithInterval = true, WithCreator = true, CreatorHasInstance = true)]
    public partial class NetHttpEventCounter : IEventCounter<NetHttpEventCounter>
    {
        [CounterItem("requests-started")]
        private ICounterPayload? requestsStarted;
        [CounterItem("requests-started-rate")]
        private ICounterPayload? requestsStartedRate;
        [CounterItem("requests-failed")]
        private ICounterPayload? requestsFailed;
        [CounterItem("requests-failed-rat")]
        private ICounterPayload? requestsFailedRat;
        [CounterItem("current-requests")]
        private ICounterPayload? currentRequests;
        [CounterItem("http11-connections-current-total")]
        private ICounterPayload? http11ConnectionsCurrentTotal;
        [CounterItem("http20-connections-current-total")]
        private ICounterPayload? http20ConnectionsCurrentTotal;
        [CounterItem("http30-connections-current-total")]
        private ICounterPayload? http30ConnectionsCurrentTotal;
        [CounterItem("http11-requests-queue-duration")]
        private ICounterPayload? http11RequestsQueueDuration;
        [CounterItem("http20-requests-queue-duration")]
        private ICounterPayload? http20RequestsQueueDuration;
        [CounterItem("http30-requests-queue-duration")]
        private ICounterPayload? http30RequestsQueueDuration;
    }
}
