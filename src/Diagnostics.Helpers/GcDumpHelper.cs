using Graphs;
using Microsoft.Diagnostics.Tools.GCDump;
using System;
using System.IO;
using System.Threading;

namespace Diagnostics.Helpers
{
    public static class GcDumpHelper
    {
        public static void WriteGcDump(int processId, string filePath, int timeout = 30, bool verbose = false, CancellationToken token = default)
        {
            using (var fs=File.Open(filePath, FileMode.Create))
            {
                WriteGcDump(processId, fs, timeout, verbose, token);
            }
        }
        public static void WriteGcDump(int processId, Stream outputStream, int timeout = 30, bool verbose = false, CancellationToken token = default)
        {
            if (TryCollectMemoryGraph(token, processId, timeout, verbose, out MemoryGraph memoryGraph))
            {
                GCHeapDump.WriteMemoryGraph(memoryGraph, outputStream);
            }
        }
        internal static bool TryCollectMemoryGraph(CancellationToken ct, int processId, int timeout, bool verbose, out MemoryGraph memoryGraph)
        {
            DotNetHeapInfo heapInfo = new();
            TextWriter log = verbose ? Console.Out : TextWriter.Null;

            memoryGraph = new MemoryGraph(50_000);

            if (!EventPipeDotNetHeapDumper.DumpFromEventPipe(ct, processId, memoryGraph, log, timeout, heapInfo))
            {
                return false;
            }

            memoryGraph.AllowReading();
            return true;
        }
    }
}
