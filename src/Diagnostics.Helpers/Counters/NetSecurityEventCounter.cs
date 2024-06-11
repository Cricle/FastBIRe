using Diagnostics.Generator.Core.Annotations;
using Diagnostics.Helpers.Annotations;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers
{
    [EventPipeProvider(WellKnowsEventProvider.NetSecurity, EventLevel.Informational)]
    [CounterMapping(ForAnysProviders = true, ForProviders = new[] { WellKnowsEventProvider.NetSecurity }, WithInterval = true, WithCreator = true, CreatorHasInstance = true)]
    public partial class NetSecurityEventCounter : IEventCounter<NetSecurityEventCounter>
    {
        [CounterItem("tls-handshake-rate")]
        private ICounterPayload? tlsHandshakeRate;
        [CounterItem("total-tls-handshakes")]
        private ICounterPayload? totalTlsHandshakes;
        [CounterItem("current-tls-handshakes")]
        private ICounterPayload? currentTlsHandshakes;
        [CounterItem("failed-tls-handshakes")]
        private ICounterPayload? failedTlsHandshakes;
        [CounterItem("all-tls-sessions-open")]
        private ICounterPayload? allTlsSessionsOpen;
        [CounterItem("tls10-sessions-open")]
        private ICounterPayload? tls10SessionsOpen;
        [CounterItem("tls11-sessions-open")]
        private ICounterPayload? tls11SessionsOpen;
        [CounterItem("tls12-sessions-open")]
        private ICounterPayload? tls12SessionsOpen;
        [CounterItem("tls13-sessions-open")]
        private ICounterPayload? tls13SessionsOpen;
        [CounterItem("all-tls-handshake-duration")]
        private ICounterPayload? allTlsHandshakeDuration;
        [CounterItem("tls10-handshake-duration")]
        private ICounterPayload? tls10HandshakeDuration;
        [CounterItem("tls11-handshake-duration")]
        private ICounterPayload? tls11HandshakeDuration;
        [CounterItem("tls12-handshake-duration")]
        private ICounterPayload? tls12HandshakeDuration;
        [CounterItem("tls13-handshake-duration")]
        private ICounterPayload? tls13HandshakeDuration;
    }
}
