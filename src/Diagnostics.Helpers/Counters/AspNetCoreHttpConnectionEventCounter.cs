using Diagnostics.Generator.Core.Annotations;
using Diagnostics.Helpers.Annotations;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers
{
    [EventPipeProvider(WellKnowsEventProvider.AspNetCoreHttpConnection, EventLevel.Informational)]
    [CounterMapping(ForAnysProviders = true, ForProviders = new[] { WellKnowsEventProvider.AspNetCoreHttpConnection }, WithInterval = true, WithCreator = true, CreatorHasInstance = true)]
    public partial class AspNetCoreHttpConnectionEventCounter : IEventCounter<AspNetCoreHttpConnectionEventCounter>
    {
        [CounterItem("connections-duration")]
        private ICounterPayload? connectionsDuration;
        [CounterItem("current-connections")]
        private ICounterPayload? currentConnections;
        [CounterItem("connections-started")]
        private ICounterPayload? connectionsStarted;
        [CounterItem("connections-stopped")]
        private ICounterPayload? connectionsStopped;
        [CounterItem("connections-timed-out")]
        private ICounterPayload? connectionsTimedOut;
    }
}
