using Diagnostics.Generator.Core.Annotations;
using Diagnostics.Helpers.Annotations;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers
{
    [EventPipeProvider(WellKnowsEventProvider.Kestrel, EventLevel.Informational)]
    [CounterMapping(ForAnysProviders = true, ForProviders = new[] { WellKnowsEventProvider.Kestrel }, WithInterval = true, WithCreator = true, CreatorHasInstance = true)]
    public partial class KestrelEventCounter : IEventCounter<KestrelEventCounter>
    {
        [CounterItem("connections-per-second")]
        private ICounterPayload? connectionsPerSecond;
        [CounterItem("total-connections")]
        private ICounterPayload? totalConnections;
        [CounterItem("tls-handshakes-per-second")]
        private ICounterPayload? tlsHandshakesPerSecond;
        [CounterItem("total-tls-handshakes")]
        private ICounterPayload? totalTlsHandshakes;
        [CounterItem("current-tls-handshakes")]
        private ICounterPayload? currentTlsHandshakes;
        [CounterItem("failed-tls-handshakes")]
        private ICounterPayload? failedTlsHandshakes;
        [CounterItem("current-connections")]
        private ICounterPayload? currentConnections;
        [CounterItem("connection-queue-length")]
        private ICounterPayload? connectionQueueLength;
        [CounterItem("request-queue-length")]
        private ICounterPayload? requestQueueLength;
        [CounterItem("current-upgraded-requests")]
        private ICounterPayload? currentUpgradedRequests;
    }
}
