using Diagnostics.Generator.Core.Annotations;
using Diagnostics.Helpers.Annotations;
using Microsoft.Diagnostics.Tracing.Parsers;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers
{
    [EventPipeProvider(WellKnowsEventProvider.Runtime, EventLevel.Informational, Keywords = (long)ClrTraceEventParser.Keywords.None)]
    [CounterMapping(ForAnysProviders = true, ForProviders = new[] { WellKnowsEventProvider.Runtime },WithInterval =true, WithCreator = true, CreatorHasInstance = true)]
    public partial class RuntimeEventCounter : IEventCounter<RuntimeEventCounter>
    {
        [CounterItem("time-in-gc")]
        private ICounterPayload? timeInGc;
        [CounterItem("alloc-rate")]
        private ICounterPayload? allocationRate;
        [CounterItem("cpu-usage")]
        private ICounterPayload? cpuUsage;
        [CounterItem("exception-count")]
        private ICounterPayload? exceptionCount;
        [CounterItem("gc-committed")]
        private ICounterPayload? gcCommittedBytes;
        [CounterItem("gc-fragmentation")]
        private ICounterPayload? gcFragmentation;
        [CounterItem("gc-heap-size")]
        private ICounterPayload? gcHeapSize;
        [CounterItem("gen-0-gc-budget")]
        private ICounterPayload? gc0Budget;
        [CounterItem("gen-0-gc-count")]
        private ICounterPayload? gc0Count;
        [CounterItem("gen-0-size")]
        private ICounterPayload? gc0Size;
        [CounterItem("gen-1-gc-count")]
        private ICounterPayload? gc1Count;
        [CounterItem("gen-1-size")]
        private ICounterPayload? gc1Size;
        [CounterItem("gen-2-gc-count")]
        private ICounterPayload? gc2Count;
        [CounterItem("gen-2-size")]
        private ICounterPayload? gc2Size;
        [CounterItem("il-bytes-jitted")]
        private ICounterPayload? ilBytesJitted;
        [CounterItem("loh-size")]
        private ICounterPayload? lohSize;
        [CounterItem("monitor-lock-contention-count")]
        private ICounterPayload? monitorLockContentionCount;
        [CounterItem("active-timer-count")]
        private ICounterPayload? numberOfActiveTimers;
        [CounterItem("assembly-count")]
        private ICounterPayload? numberOfAssembliesLoaded;
        [CounterItem("methods-jitted-count")]
        private ICounterPayload? numberOfMethodsJitted;
        [CounterItem("poh-size")]
        private ICounterPayload? pohSize;
        [CounterItem("threadpool-completed-items-count")]
        private ICounterPayload? threadPoolCompletedWorkItemCount;
        [CounterItem("threadpool-queue-length")]
        private ICounterPayload? threadPoolQueueLength;
        [CounterItem("threadpool-thread-count")]
        private ICounterPayload? threadPoolThreadCount;
        [CounterItem("total-pause-time-by-gc")]
        private ICounterPayload? timePausedByGC;
        [CounterItem("time-in-jit")]
        private ICounterPayload? timeSpentInJIT;
        [CounterItem("working-set")]
        private ICounterPayload? workingSet;
    }
}
