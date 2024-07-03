using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Diagnostics.Helpers
{
    public class HeapHelper : IDisposable
    {
        public class GCHeapStatistics
        {
            internal ulong count;
            internal ulong size;

            public GCHeapStatistics(ClrType type)
            {
                Type = type;
            }

            public ClrType Type { get; }

            public ulong Count => count;

            public ulong Size => size;

            public override string ToString()
            {
                return $"{{{Type.Name},Count={count},Size={size}}}";
            }
        }


        private readonly Dictionary<ClrType, GCHeapStatistics> stats = new Dictionary<ClrType, GCHeapStatistics>();

        public HeapHelper(ClrRuntime runtime)
        {
            Runtime = runtime;
        }

        public ClrRuntime Runtime { get; }

        public int GCHeapStatisticsCount => stats.Count;

        public IReadOnlyDictionary<ClrType, GCHeapStatistics> GCHeapResult => stats;

        public ulong TotalCount => (ulong)stats.Values.Sum(x => (long)x.count);

        public ulong TotalSize=> (ulong)stats.Values.Sum(x => (long)x.size);
        
        public IEnumerable<GCHeapStatistics> OrderDescendingMaxHeaps()
        {
            return Runtime.Heap.EnumerateObjects().Where(x => x.Type != null)
                .GroupBy(x => x.Type)
                .OrderByDescending(x => x.Sum(y => (long)y.Size)).ThenByDescending(x => x.Count())
                .Select(x => new GCHeapStatistics(x.Key!)
                {
                    count = (ulong)x.LongCount(),
                    size = (ulong)x.Sum(x => (long)x.Size)
                });
        }
        public IQueryable<GCHeapStatistics> GetMaxHeaps(int max, Expression<Func<KeyValuePair<ClrType, GCHeapStatistics>, bool>>? filters = null)
        {
            var query = stats.AsQueryable();
            if (filters != null)
            {
                query = query.Where(filters);
            }
            return query.OrderByDescending(x => x.Value.size).ThenByDescending(x => x.Value.count).Select(x => x.Value).Take(max);
        }

        public void AnalyzeGcHeap()
        {
            stats.Clear();
            foreach (ClrObject obj in Runtime.Heap.EnumerateObjects())
            {
                if (obj.IsValid && obj.Type?.MethodTable != null && obj.Type != null)
                {
                    if (!stats.TryGetValue(obj.Type, out var item))
                    {
                        item = new GCHeapStatistics(obj.Type);
                        stats[obj.Type] = item;
                    }
                    item.count++;
                    item.size += obj.Size;
                }
            }
        }

        public void Dispose()
        {
            Runtime.Dispose();
        }
    }
}
