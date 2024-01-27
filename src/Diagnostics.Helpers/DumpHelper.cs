using Microsoft.Diagnostics.NETCore.Client;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public static class DumpHelper
    {
        public static Task DumpSelfAsync(string outputPath)
        {
            return DumpAsync(PlatformHelper.CurrentProcessId, outputPath);
        }
        public static Task DumpAsync(string processName, string outputPath)
        {
            var proc = Process.GetProcessesByName(processName);
            if (proc.Length == 0)
            {
                throw new ArgumentException($"Process {proc} not found");
            }
            return DumpAsync(proc[0].Id, outputPath);
        }
        public static Task DumpAsync(int processId, string outputPath, DumpType dumpType = DumpType.Full, WriteDumpFlags dumpFlags = WriteDumpFlags.None, CancellationToken token = default)
        {
            var client = new DiagnosticsClient(processId);
            return client.WriteDumpAsync(dumpType, outputPath, dumpFlags, token);
        }
    }
}
