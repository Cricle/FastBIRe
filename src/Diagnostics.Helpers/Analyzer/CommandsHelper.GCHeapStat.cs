using Diagnostics.Helpers.Analyzer.Output;
using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Diagnostics.Helpers.Analyzer
{
    public static partial class CommandsHelper
    {
        public static HeapInfo[] GCHeapStat(ClrRuntime runtime,TextWriter? textWriter=null,bool includeUnreachable=false,CancellationToken token=default)
        {
            var liv = new LiveObjectService(new RootCacheService(runtime, token), runtime, token);
            var heaps = runtime.Heap.SubHeaps.Select(h => GetHeapInfo(h, liv, includeUnreachable)).ToArray();
            if (textWriter==null)
            {
                return heaps;
            }
            var printFrozen = heaps.Any(h => h.Frozen.Committed != 0);

            var formats = new List<Column>(10)
            {
                ColumnKind.Text.WithWidth(8),
                ColumnKind.IntegerWithoutCommas,
                ColumnKind.IntegerWithoutCommas,
                ColumnKind.IntegerWithoutCommas,
                ColumnKind.IntegerWithoutCommas,
                ColumnKind.IntegerWithoutCommas,
                ColumnKind.Text.WithWidth(8),
                ColumnKind.Text.WithWidth(8),
                ColumnKind.Text.WithWidth(8)
            };

            if (printFrozen)
            {
                formats.Insert(1, ColumnKind.IntegerWithoutCommas);
            }

            Table output = new(textWriter ?? TextWriter.Null, formats.ToArray());
            output.SetAlignment(Align.Left);

            WriteHeader(output, heaps, printFrozen);// Write allocated
            foreach (HeapInfo heapInfo in heaps)
            {
                WriteRow(output, heapInfo, (info) => info.Allocated, printFrozen);
            }

            HeapInfo total = GetTotal(heaps);
            WriteRow(output, total, (info) => info.Allocated, printFrozen);
            textWriter?.WriteLine();

            // Write Free
            textWriter?.WriteLine("Free space:");
            WriteHeader(output, heaps, printFrozen);
            foreach (HeapInfo heapInfo in heaps)
            {
                WriteRow(output, heapInfo, (info) => info.Free, printFrozen, printPercentage: true);
            }

            total = GetTotal(heaps);
            WriteRow(output, total, (info) => info.Free, printFrozen);
            textWriter?.WriteLine();

            // Write unrooted
            if (includeUnreachable)
            {
                textWriter?.WriteLine("Unrooted objects:");
                WriteHeader(output, heaps, printFrozen);
                foreach (HeapInfo heapInfo in heaps)
                {
                    WriteRow(output, heapInfo, (info) => info.Unrooted, printFrozen, printPercentage: true);
                }
                textWriter?.WriteLine();

                total = GetTotal(heaps);
                WriteRow(output, total, (info) => info.Unrooted, printFrozen);
                textWriter?.WriteLine();
            }
            // Write Committed
            textWriter?.WriteLine("Committed space:");
            WriteHeader(output, heaps, printFrozen);
            foreach (HeapInfo heapInfo in heaps)
            {
                WriteRow(output, heapInfo, (info) => info.Committed, printFrozen);
            }

            total = GetTotal(heaps);
            WriteRow(output, total, (info) => info.Committed, printFrozen, printPercentage: false, footer: true);
            textWriter?.WriteLine();
            return heaps;
        }
        private static ulong GetValue(object value)
        {
            if (value is ulong ul)
            {
                return ul;
            }

            return 0;
        }
        private static void WriteRow(Table output, HeapInfo heapInfo, Func<GenerationInfo, object> select, bool printFrozen, bool printPercentage = false, bool footer = false)
        {
            List<object> row = new(11)
            {
                heapInfo.Index == -1 ? "Total" : $"Heap{heapInfo.Index}",
                select(heapInfo.Gen0),
                select(heapInfo.Gen1),
                select(heapInfo.Gen2),
                select(heapInfo.LoH),
                select(heapInfo.PoH),
            };

            if (printFrozen)
            {
                select(heapInfo.Frozen);
            }

            bool hasEphemeral = heapInfo.Ephemeral.Committed > 0;
            if (hasEphemeral)
            {
                row.Insert(1, select(heapInfo.Ephemeral));
            }

            if (printPercentage)
            {
                ulong allocated = heapInfo.Gen0.Allocated + heapInfo.Gen1.Allocated + heapInfo.Gen2.Allocated;
                if (allocated != 0)
                {
                    ulong value = GetValue(select(heapInfo.Gen0)) + GetValue(select(heapInfo.Gen1)) + GetValue(select(heapInfo.Gen2));
                    ulong percent = value * 100 / allocated;
                    row.Add($"SOH:{percent}%");
                }
                else
                {
                    row.Add(null);
                }

                if (heapInfo.LoH.Allocated != 0)
                {
                    ulong percent = GetValue(select(heapInfo.LoH)) * 100 / heapInfo.LoH.Allocated;
                    row.Add($"LOH:{percent}%");
                }
                else
                {
                    row.Add(null);
                }

                if (heapInfo.PoH.Allocated != 0)
                {
                    ulong percent = GetValue(select(heapInfo.PoH)) * 100 / heapInfo.PoH.Allocated;
                    row.Add($"POH:{percent}%");
                }
                else
                {
                    row.Add(null);
                }
            }

            if (footer)
            {
                output.WriteFooter(row.ToArray());
            }
            else
            {
                output.WriteRow(row.ToArray());
            }
        }

        private static void WriteHeader(Table output, HeapInfo[] heaps, bool printFrozen)
        {
            List<string> row = new(8) { "Heap", "Gen0", "Gen1", "Gen2", "LOH", "POH" };

            if (printFrozen)
            {
                row.Add("FRZ");
            }

            bool hasEphemeral = heaps.Any(h => h.Ephemeral.Committed > 0);
            if (hasEphemeral)
            {
                row.Insert(1, "EPH");
            }

            output.WriteHeader(row.ToArray());
        }

        private static HeapInfo GetTotal(HeapInfo[] heaps)
        {
            HeapInfo total = new();
            foreach (HeapInfo heap in heaps)
            {
                total += heap;
            }

            return total;
        }
        private static HeapInfo GetHeapInfo(ClrSubHeap heap,LiveObjectService liveObjectService,bool includeUnreachable)
        {
            HeapInfo result = new()
            {
                Index = heap.Index,
            };

            foreach (ClrSegment seg in heap.Segments)
            {
                if (seg.Kind == GCSegmentKind.Ephemeral)
                {
                    result.Ephemeral.Allocated += seg.ObjectRange.Length;
                    result.Ephemeral.Committed += seg.CommittedMemory.Length;

                    foreach (ClrObject obj in seg.EnumerateObjects(carefully: true))
                    {
                        // Ignore heap corruption
                        if (!obj.IsValid)
                        {
                            continue;
                        }

                        GenerationInfo genInfo = result.GetInfoByGeneration(seg.GetGeneration(obj));
                        if (genInfo is not null)
                        {
                            if (obj.IsFree)
                            {
                                result.Ephemeral.Free += obj.Size;
                                genInfo.Free += obj.Size;
                            }
                            else
                            {
                                genInfo.Allocated += obj.Size;

                                if (includeUnreachable && !liveObjectService.IsLive(obj))
                                {
                                    genInfo.Unrooted += obj.Size;
                                }
                            }
                        }
                    }
                }
                else
                {
                    GenerationInfo? info = seg.Kind switch
                    {
                        GCSegmentKind.Generation0 => result.Gen0,
                        GCSegmentKind.Generation1 => result.Gen1,
                        GCSegmentKind.Generation2 => result.Gen2,
                        GCSegmentKind.Large => result.LoH,
                        GCSegmentKind.Pinned => result.PoH,
                        GCSegmentKind.Frozen => result.Frozen,
                        _ => null
                    };

                    if (info is not null)
                    {
                        info.Allocated += seg.ObjectRange.Length;
                        info.Committed += seg.CommittedMemory.Length;

                        foreach (ClrObject obj in seg.EnumerateObjects(carefully: true))
                        {
                            // Ignore heap corruption
                            if (!obj.IsValid)
                            {
                                continue;
                            }

                            if (obj.IsFree)
                            {
                                info.Free += obj.Size;
                            }
                            else if (includeUnreachable && !liveObjectService.IsLive(obj))
                            {
                                info.Unrooted += obj.Size;
                            }
                        }
                    }
                }
            }

            return result;
        }

    }
}
