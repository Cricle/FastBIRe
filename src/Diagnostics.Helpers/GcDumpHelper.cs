using Graphs;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Tools.GCDump;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{

    public static class GcDumpHelper
    {
        public static void WriteGcDump(int processId, string filePath, int timeout = 30, TextWriter? log = null, CancellationToken token = default)
        {
            using (var fs = File.Open(filePath, FileMode.Create))
            {
                WriteGcDump(processId, fs, timeout, log, false, token);
            }
        }
        public static void WriteGcDump(int processId, Stream outputStream, int timeout = 30, TextWriter? log = null,bool leaveOpen=false, CancellationToken token = default)
        {
            if (TryCollectMemoryGraph(processId, timeout, log, out MemoryGraph memoryGraph, token))
            {
                GCHeapDump.WriteMemoryGraph(memoryGraph, outputStream, leaveOpen: leaveOpen);
            }
        }
        public static bool TryCollectMemoryGraph(int processId, int timeout, TextWriter? log, out MemoryGraph memoryGraph, CancellationToken ct)
        {
            DotNetHeapInfo heapInfo = new();
            log ??= TextWriter.Null;

            memoryGraph = new MemoryGraph(50_000);

            if (!EventPipeDotNetHeapDumper.DumpFromEventPipe(ct, processId, memoryGraph, log, timeout, heapInfo))
            {
                return false;
            }

            memoryGraph.AllowReading();
            return true;
        }
        public static async Task WriteAsync(this MemoryGraph memoryGraph,TextWriter writer)
        {
            // Print summary
            await WriteSummaryRowAsync(memoryGraph.TotalSize, "GC Heap bytes",writer);
            await WriteSummaryRowAsync(memoryGraph.NodeCount, "GC Heap objects", writer);

            if (memoryGraph.TotalNumberOfReferences > 0)
            {
                await WriteSummaryRowAsync(memoryGraph.TotalNumberOfReferences, "Total references", writer);
            }

            await writer.WriteLineAsync();

            // Print Details
            await writer.WriteAsync($"{"Object Bytes",15:N0}");
            await writer.WriteAsync($"  {"Count",8:N0}");
            await writer.WriteAsync("  Type");
            await writer.WriteLineAsync();

            IOrderedEnumerable<ReportItem> filteredTypes = GetReportItem(memoryGraph)
                .OrderByDescending(t => t.SizeBytes)
                .ThenByDescending(t => t.Count);

            foreach (ReportItem filteredType in filteredTypes)
            {
                await writer.WriteAsync($"{filteredType.SizeBytes,15:N0}");
                await writer.WriteAsync("  ");
                if (filteredType.Count.HasValue)
                {
                    await writer.WriteAsync($"{filteredType.Count.Value,8:N0}");
                    await writer.WriteAsync("  ");
                }
                else
                {
                    await writer.WriteAsync($"{"",8}  ");
                }

                await writer.WriteAsync(filteredType.TypeName ?? "<UNKNOWN>");
                var dllName = GetDllName(filteredType.ModuleName ?? "");
                if (dllName.Length!=0)
                {
                    await writer.WriteAsync("  ");
                    await writer.WriteAsync('[');
                    await writer.WriteAsync(GetDllName(filteredType.ModuleName ?? ""));
                    await writer.WriteAsync(']');
                }

                await writer.WriteLineAsync();
            }

            static string GetDllName(string input)
                => input.Substring(input.LastIndexOf(Path.DirectorySeparatorChar) + 1);


        }
        static async ValueTask WriteSummaryRowAsync(object value, string text,TextWriter writer)
        {
            await writer.WriteAsync($"{value,15:N0}  ");
            await writer.WriteAsync(text);
            await writer.WriteLineAsync();
        }

        public static IEnumerable<ReportItem> GetReportItem(MemoryGraph memoryGraph)
        {
            Graph.SizeAndCount[] histogramByType = memoryGraph.GetHistogramByType();
            for (int index = 0; index < memoryGraph.m_types.Count; index++)
            {
                Graph.TypeInfo type = memoryGraph.m_types[index];
                if (string.IsNullOrEmpty(type.Name) || type.Size == 0)
                {
                    continue;
                }

                Graph.SizeAndCount? sizeAndCount = histogramByType.FirstOrDefault(c => (int)c.TypeIdx == index);
                if (sizeAndCount == null || sizeAndCount.Count == 0)
                {
                    continue;
                }

                yield return new ReportItem
                {
                    TypeName = type.Name,
                    ModuleName = type.ModuleName,
                    SizeBytes = type.Size,
                    Count = sizeAndCount.Count
                };
            }
        }
    }
    public struct ReportItem
    {
        public int? Count { get; set; }
        public long SizeBytes { get; set; }
        public string TypeName { get; set; }
        public string ModuleName { get; set; }
    }

}
