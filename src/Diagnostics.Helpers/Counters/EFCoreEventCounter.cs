using Diagnostics.Generator.Core.Annotations;
using Diagnostics.Helpers.Annotations;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers
{
    [EventPipeProvider(WellKnowsEventProvider.EFCorePrivate, EventLevel.Informational)]
    [CounterMapping(ForAnysProviders = true, ForProviders = new[] { WellKnowsEventProvider.EFCorePrivate }, WithInterval = true, WithCreator = true, CreatorHasInstance = true)]
    public partial class EFCoreEventCounter : IEventCounter<EFCoreEventCounter>
    {
        [CounterItem("active-db-contexts")]
        private ICounterPayload? activeDbContexts;
        [CounterItem("total-queries")]
        private ICounterPayload? totalQueries;
        [CounterItem("queries-per-second")]
        private ICounterPayload? queriesPerSecond;
        [CounterItem("total-save-changes")]
        private ICounterPayload? totalSaveChanges;
        [CounterItem("save-changes-per-second")]
        private ICounterPayload? saveChangesPerSecond;
        [CounterItem("compiled-query-cache-hit-rate")]
        private ICounterPayload? compiledQueryCacheHitRate;
        [CounterItem("total-execution-strategy-operation-failures")]
        private ICounterPayload? totalExecutionStrategyOperationFailures;
        [CounterItem("execution-strategy-operation-failures-per-second")]
        private ICounterPayload? executionStrategyOperationFailuresPerSecond;
        [CounterItem("total-optimistic-concurrency-failures")]
        private ICounterPayload? totalOtimisticConcurrencyFailures;
        [CounterItem("optimistic-concurrency-failures-per-second")]
        private ICounterPayload? optimisticConcurrencyFailuresPerSecond;
    }
}
