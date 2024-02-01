using Graphs;
using Microsoft.Diagnostics.Tools.GCDump;
using System.IO;
using System.Threading;

namespace Diagnostics.Helpers
{
    public static class GcDumpHelper
    {
        public static void WriteGcDump(int processId, string filePath, int timeout = 30, TextWriter? log = null, CancellationToken token = default)
        {
            using (var fs = File.Open(filePath, FileMode.Create))
            {
                WriteGcDump(processId, fs, timeout, log, token);
            }
        }
        public static void WriteGcDump(int processId, Stream outputStream, int timeout = 30, TextWriter? log = null, CancellationToken token = default)
        {
            if (TryCollectMemoryGraph(processId, timeout, log, out MemoryGraph memoryGraph, token))
            {
                GCHeapDump.WriteMemoryGraph(memoryGraph, outputStream);
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
    }
}
