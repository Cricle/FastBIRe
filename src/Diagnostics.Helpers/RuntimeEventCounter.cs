using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Diagnostics.Helpers
{
    public class RuntimeEventCounter : ICloneable
    {
        private ICounterPayload timeInGc;
        private ICounterPayload allocationRate;
        private ICounterPayload cpuUsage;
        private ICounterPayload exceptionCount;
        private ICounterPayload gcCommittedBytes;
        private ICounterPayload gcFragmentation;
        private ICounterPayload gcHeapSize;
        private ICounterPayload gc0Budget;
        private ICounterPayload gc0Count;
        private ICounterPayload gc0Size;
        private ICounterPayload gc1Count;
        private ICounterPayload gc1Size;
        private ICounterPayload gc2Count;
        private ICounterPayload gc2Size;
        private ICounterPayload ilBytesJitted;
        private ICounterPayload lohSize;
        private ICounterPayload monitorLockContentionCount;
        private ICounterPayload numberOfActiveTimers;
        private ICounterPayload numberOfAssembliesLoaded;
        private ICounterPayload numberOfMethodsJitted;
        private ICounterPayload pohSize;
        private ICounterPayload threadPoolCompletedWorkItemCount;
        private ICounterPayload threadPoolQueueLength;
        private ICounterPayload threadPoolThreadCount;
        private ICounterPayload timePausedByGC;
        private ICounterPayload timeSpentInJIT;
        private ICounterPayload workingSet;

        public ICounterPayload AllocationRate => Volatile.Read(ref allocationRate);
        public ICounterPayload CPUUsage => Volatile.Read(ref cpuUsage);
        public ICounterPayload ExceptionCount => Volatile.Read(ref exceptionCount);
        public ICounterPayload GCCommittedBytes => Volatile.Read(ref gcCommittedBytes);
        public ICounterPayload GCFragmentation => Volatile.Read(ref gcFragmentation);
        public ICounterPayload GcHeapSize => Volatile.Read(ref gcHeapSize);
        public ICounterPayload GC0Budget => Volatile.Read(ref gc0Budget);
        public ICounterPayload GC0Count => Volatile.Read(ref gc0Count);
        public ICounterPayload GC0Size => Volatile.Read(ref gc0Size);
        public ICounterPayload GC1Count => Volatile.Read(ref gc1Count);
        public ICounterPayload GC1Size => Volatile.Read(ref gc1Size);
        public ICounterPayload GC2Count => Volatile.Read(ref gc2Count);
        public ICounterPayload GC2Size => Volatile.Read(ref gc2Size);
        public ICounterPayload ILBytesJitted => Volatile.Read(ref ilBytesJitted);
        public ICounterPayload LOHSize => Volatile.Read(ref lohSize);
        public ICounterPayload MonitorLockContentionCount => Volatile.Read(ref monitorLockContentionCount);
        public ICounterPayload NumberOfActiveTimers => Volatile.Read(ref numberOfActiveTimers);
        public ICounterPayload NumberOfAssembliesLoaded => Volatile.Read(ref numberOfAssembliesLoaded);
        public ICounterPayload NumberOfMethodsJitted => Volatile.Read(ref numberOfMethodsJitted);
        public ICounterPayload PohSize => Volatile.Read(ref pohSize);
        public ICounterPayload ThreadPoolCompletedWorkItemCount => Volatile.Read(ref threadPoolCompletedWorkItemCount);
        public ICounterPayload ThreadPoolQueueLength => Volatile.Read(ref threadPoolQueueLength);
        public ICounterPayload ThreadPoolThreadCount => Volatile.Read(ref threadPoolThreadCount);
        public ICounterPayload TimePausedByGC => Volatile.Read(ref timePausedByGC);
        public ICounterPayload TimeSpentInJIT => Volatile.Read(ref timeSpentInJIT);
        public ICounterPayload WorkingSet => Volatile.Read(ref workingSet);
        public ICounterPayload TimeInGc => Volatile.Read(ref timeInGc);

        public event EventHandler? Changed;

        protected void RaiseChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public RuntimeEventCounter Copy()
        {
            return (RuntimeEventCounter)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        public void Update(ICounterPayload payload)
        {
            if (payload.Name == "cpu-usage")
                Volatile.Write(ref cpuUsage, payload);
            else if (payload.Name == "working-set")
                Volatile.Write(ref workingSet, payload);
            else if (payload.Name == "gen-0-gc-count")
                Volatile.Write(ref gc0Count, payload);
            else if (payload.Name == "gen-1-gc-count")
                Volatile.Write(ref gc1Count, payload);
            else if (payload.Name == "gen-2-gc-count")
                Volatile.Write(ref gc2Count, payload);
            else if (payload.Name == "threadpool-thread-count")
                Volatile.Write(ref threadPoolThreadCount, payload);
            else if (payload.Name == "monitor-lock-contention-count")
                Volatile.Write(ref monitorLockContentionCount, payload);
            else if (payload.Name == "threadpool-completed-items-count")
                Volatile.Write(ref threadPoolCompletedWorkItemCount, payload);
            else if (payload.Name == "alloc-rate")
                Volatile.Write(ref allocationRate, payload);
            else if (payload.Name == "active-timer-count")
                Volatile.Write(ref numberOfActiveTimers, payload);
            else if (payload.Name == "gc-fragmentation")
                Volatile.Write(ref gcFragmentation, payload);
            else if (payload.Name == "gc-committed")
                Volatile.Write(ref gcCommittedBytes, payload);
            else if (payload.Name == "exception-count")
                Volatile.Write(ref exceptionCount, payload);
            else if (payload.Name == "time-in-gc")
                Volatile.Write(ref timeInGc, payload);
            else if (payload.Name == "total-pause-time-by-gc")
                Volatile.Write(ref timePausedByGC, payload);
            else if (payload.Name == "gen-0-size")
                Volatile.Write(ref gc0Size, payload);
            else if (payload.Name == "gen-1-size")
                Volatile.Write(ref gc1Size, payload);
            else if (payload.Name == "gen-2-size")
                Volatile.Write(ref gc2Size, payload);
            else if (payload.Name == "loh-size")
                Volatile.Write(ref lohSize, payload);
            else if (payload.Name == "poh-size")
                Volatile.Write(ref pohSize, payload);
            else if (payload.Name == "assembly-count")
                Volatile.Write(ref numberOfAssembliesLoaded, payload);
            else if (payload.Name == "il-bytes-jitted")
                Volatile.Write(ref ilBytesJitted, payload);
            else if (payload.Name == "time-in-jit")
                Volatile.Write(ref timeSpentInJIT, payload);
            else if (payload.Name == "gen-0-gc-budget")
                Volatile.Write(ref gc0Budget, payload);
            else if (payload.Name == "gc-heap-size")
                Volatile.Write(ref gcHeapSize, payload);
            else if (payload.Name == "gen-1-count")
                Volatile.Write(ref gc1Count, payload);
            else if (payload.Name == "gen-1-count")
                Volatile.Write(ref gc2Count, payload);
            else if (payload.Name == "methods-jitted-count")
                Volatile.Write(ref numberOfMethodsJitted, payload);
            else if (payload.Name == "threadpool-queue-length")
                Volatile.Write(ref threadPoolQueueLength, payload);
            else
                return;
            OnUpdated(payload);
        }

        protected virtual void OnUpdated(ICounterPayload payload)
        {
            RaiseChanged();
        }

        public void WriteTo(TextWriter sb)
        {
            sb.WriteLine("% Time in GC since last GC (%)\t\t\t\t\t{0}", TimeInGc?.Value);
            sb.WriteLine("Allocation Rate (B / 1 sec)\t\t\t\t\t{0}", AllocationRate?.Value);
            sb.WriteLine("CPU Usage (%)\t\t\t\t\t\t\t{0}", CPUUsage?.Value);
            sb.WriteLine("Exception Count (Count / 1 sec)\t\t\t\t\t{0}", ExceptionCount?.Value);
            sb.WriteLine("GC Committed Bytes (MB)\t\t\t\t\t\t{0}", GCCommittedBytes?.Value);
            sb.WriteLine("GC Fragmentation (%)\t\t\t\t\t\t{0}", GCFragmentation?.Value);
            sb.WriteLine("GC Heap Size (MB)\t\t\t\t\t\t{0}", GcHeapSize?.Value);
            sb.WriteLine("Gen 0 GC Budget (MB)\t\t\t\t\t\t{0}", GC0Budget?.Value);
            sb.WriteLine("Gen 0 GC Count (Count / 1 sec)\t\t\t\t\t{0}", GC0Count?.Value);
            sb.WriteLine("Gen 0 Size (B)\t\t\t\t\t\t\t{0}", GC1Size?.Value);
            sb.WriteLine("Gen 1 GC Count (Count / 1 sec)\t\t\t\t\t{0}", GC1Count?.Value);
            sb.WriteLine("Gen 1 Size (B)\t\t\t\t\t\t\t{0}", GC1Size?.Value);
            sb.WriteLine("Gen 2 GC Count (Count / 1 sec)\t\t\t\t\t{0}", GC2Count?.Value);
            sb.WriteLine("Gen 2 Size (B)\t\t\t\t\t\t\t{0}", GC2Size?.Value);
            sb.WriteLine("IL Bytes Jitted (B)\t\t\t\t\t\t{0}", ILBytesJitted?.Value);
            sb.WriteLine("LOH Size (B)\t\t\t\t\t\t\t{0}", LOHSize?.Value);
            sb.WriteLine("Monitor Lock Contention Count (Count / 1 sec)\t\t\t{0}", MonitorLockContentionCount?.Value);
            sb.WriteLine("Number of Active Timers\t\t\t\t\t\t{0}", NumberOfActiveTimers?.Value);
            sb.WriteLine("Number of Assemblies Loaded\t\t\t\t\t{0}", NumberOfAssembliesLoaded?.Value);
            sb.WriteLine("Number of Methods Jitted\t\t\t\t\t{0}", NumberOfMethodsJitted?.Value);
            sb.WriteLine("POH (Pinned Object Heap) Size (B)\t\t\t\t{0}", PohSize?.Value);
            sb.WriteLine("ThreadPool Completed Work Item Count (Count / 1 sec)\t\t{0}", ThreadPoolCompletedWorkItemCount?.Value);
            sb.WriteLine("ThreadPool Queue Length\t\t\t\t\t\t{0}", ThreadPoolQueueLength?.Value);
            sb.WriteLine("ThreadPool Thread Count\t\t\t\t\t\t{0}", ThreadPoolThreadCount?.Value);
            sb.WriteLine("Time paused by GC (ms / 1 sec)\t\t\t\t\t{0}", TimePausedByGC?.Value);
            sb.WriteLine("Time spent in JIT (ms / 1 sec)\t\t\t\t\t{0}", TimeSpentInJIT?.Value);
            sb.WriteLine("Working Set (MB)\t\t\t\t\t\t{0}", WorkingSet?.Value);
        }

        public override string ToString()
        {
            var sb = new StringWriter();
            WriteTo(sb);
            return sb.ToString();
        }
    }
}
