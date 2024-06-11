using Diagnostics.Generator.Core.Annotations;
using Diagnostics.Helpers.Annotations;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers
{
    [EventPipeProvider(WellKnowsEventProvider.AspNetCoreHosting, EventLevel.Informational)]
    [CounterMapping(ForAnysProviders = true, ForProviders = new[] { WellKnowsEventProvider.AspNetCoreHosting }, WithInterval = true,WithCreator =true, CreatorHasInstance =true)]
    public partial class AspNetCoreHostingEventCounter : IEventCounter<AspNetCoreHostingEventCounter>
    {
        [CounterItem("requests-per-second")]
        private ICounterPayload? requestPreSecond;
        [CounterItem("current-requests")]
        private ICounterPayload? concurrentRequests;
        [CounterItem("failed-requests")]
        private ICounterPayload? failedRequests;
        [CounterItem("total-requests")]
        private ICounterPayload? totalRequests;
    }
}
