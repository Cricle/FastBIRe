using Microsoft.Diagnostics.NETCore.Client;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public static partial class DumpHelper
    {
        private static string GetOutputName(string processName)
        {
            return $"{processName}_{DateTime.Now:yyyy-MM-dd HH:mm:ss}.dmp";
        }

        public static void DumpSelf(string? outputPath=null)
        {
            outputPath ??= GetOutputName(Process.GetCurrentProcess().ProcessName);
            Dump(PlatformHelper.CurrentProcessId, outputPath);
        }
        public static void Dump(string processName, string? outputPath=null)
        {
            outputPath ??= GetOutputName(processName);
            var proc = Process.GetProcessesByName(processName);
            if (proc.Length == 0)
            {
                throw new ArgumentException($"Process {proc} not found");
            }
            Dump(proc[0].Id, outputPath);
        }
        public static void Dump(int processId,
            string outputPath,
            DumpTypeOption option = DumpTypeOption.Full,
            WriteDumpFlags dumpFlags = WriteDumpFlags.None,
            bool logger = false,
            bool crashReport = false)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)&&!crashReport)
            {
                Windows.CollectDump(processId, outputPath, option);
            }
            else
            {
                DiagnosticsClient client = new(processId);

                var dumpType = DumpType.Normal;
                switch (option)
                {
                    case DumpTypeOption.Full:
                        dumpType = DumpType.Full;
                        break;
                    case DumpTypeOption.Heap:
                        dumpType = DumpType.WithHeap;
                        break;
                    case DumpTypeOption.Mini:
                        dumpType = DumpType.Normal;
                        break;
                    case DumpTypeOption.Triage:
                        dumpType = DumpType.Triage;
                        break;
                }

                WriteDumpFlags flags = WriteDumpFlags.None;
                if (logger)
                {
                    flags |= WriteDumpFlags.LoggingEnabled;
                }
                if (crashReport)
                {
                    flags |= WriteDumpFlags.CrashReportEnabled;
                }
                // Send the command to the runtime to initiate the core dump
                client.WriteDump(dumpType, outputPath, flags);
            }
        }
    }
}
