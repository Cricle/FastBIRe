using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Diagnostics.Helpers.Analyzer
{
    public class LiveObjectService
    {
        private HashSet<ulong>? _liveObjs;
        public LiveObjectService(ClrRuntime runtime, CancellationToken cancellationToken)
            :this(new RootCacheService(runtime,cancellationToken),runtime,cancellationToken)
        {

        }
        public LiveObjectService(RootCacheService rootCache, ClrRuntime runtime, CancellationToken cancellationToken)
        {
            RootCache = rootCache;
            Runtime = runtime;
            CancellationToken = cancellationToken;
        }

        public int UpdateSeconds { get; set; } = 15;

        public bool PrintWarning { get; set; } = true;

        public RootCacheService RootCache { get; }

        public ClrRuntime Runtime { get;  }

        public CancellationToken CancellationToken { get; }

        public bool IsLive(ClrObject obj) => IsLive(obj.Address);

        public bool IsLive(ulong obj, TextWriter? writer = null)
        {
            _liveObjs ??= CreateObjectSet(writer);
            return _liveObjs.Contains(obj);
        }

        public void Initialize(TextWriter? writer = null)
        {
            _liveObjs ??= CreateObjectSet(writer);
        }

        private HashSet<ulong> CreateObjectSet(TextWriter? writer=null)
        {
            ClrHeap heap = Runtime.Heap;
            HashSet<ulong> live = new();

            Stopwatch sw = Stopwatch.StartNew();
            int updateSeconds = Math.Max(UpdateSeconds, 10);
            bool printWarning = PrintWarning;

            if (printWarning)
            {
                writer?.WriteLine("Calculating live objects, this may take a while...");
            }

            int roots = 0;
            Queue<ulong> todo = new();
            foreach (ClrRoot root in RootCache.EnumerateRoots())
            {
                roots++;
                if (printWarning && sw.Elapsed.TotalSeconds > updateSeconds && live.Count > 0)
                {
                    writer?.WriteLine($"Calculating live objects: {live.Count:n0} found");
                    sw.Restart();
                }

                if (live.Add(root.Object))
                {
                    todo.Enqueue(root.Object);
                }
            }

            // We calculate the % complete based on how many are left in our todo queue.
            // This means that % complete can go down if we end up seeing an unexpectedly
            // high number of references compared to earlier objects.
            int maxCount = todo.Count;
            while (todo.Count > 0)
            {
                if (printWarning && sw.Elapsed.TotalSeconds > updateSeconds)
                {
                    if (todo.Count > maxCount)
                    {
                        writer?.WriteLine($"Calculating live objects: {live.Count:n0} found");
                    }
                    else
                    {
                        writer?.WriteLine($"Calculating live objects: {live.Count:n0} found - {(maxCount - todo.Count) * 100 / (float)maxCount:0.0}% complete");
                    }

                    maxCount = Math.Max(maxCount, todo.Count);
                    sw.Restart();
                }

                CancellationToken.ThrowIfCancellationRequested();

                ulong currAddress = todo.Dequeue();
                ClrObject obj = heap.GetObject(currAddress);

                foreach (ulong address in obj.EnumerateReferenceAddresses(carefully: false, considerDependantHandles: true))
                {
                    if (live.Add(address))
                    {
                        todo.Enqueue(address);
                    }
                }
            }

            if (printWarning)
            {
                writer?.WriteLine($"Calculating live objects complete: {live.Count:n0} objects from {roots:n0} roots");
            }

            return live;
        }
    }
}
