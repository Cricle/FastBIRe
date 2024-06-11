using Diagnostics.Generator.Core.Annotations;
using Diagnostics.Helpers.Annotations;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers
{
    [EventPipeProvider(WellKnowsEventProvider.NetSockets, EventLevel.Informational)]
    [CounterMapping(ForAnysProviders = true, ForProviders = new[] { WellKnowsEventProvider.NetSockets }, WithInterval = true, WithCreator = true, CreatorHasInstance = true)]
    public partial class NetSocketsEventCounter : IEventCounter<NetSocketsEventCounter>
    {
        [CounterItem("outgoing-connections-established")]
        private ICounterPayload? outgoingConnectionsEstablished;
        [CounterItem("incoming-connections-established")]
        private ICounterPayload? incomingConnectionsEstablished;
        [CounterItem("current-outgoing-connect-attempts")]
        private ICounterPayload? currentOutgoingConnectAttempts;
        [CounterItem("bytes-received")]
        private ICounterPayload? bytesReceived;
        [CounterItem("bytes-sent")]
        private ICounterPayload? bytesSent;
        [CounterItem("datagrams-received")]
        private ICounterPayload? datagramsReceived;
        [CounterItem("datagrams-sent")]
        private ICounterPayload? datagramsSent;
    }
}
