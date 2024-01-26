using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public static partial class DumpHelper
    {
        private static readonly IMemoryDumper windowDumper = new WindowsMemoryDumper();
        private static readonly IMemoryDumper linuxDumper = new LinuxMemoryDumper();

        public static bool IsSupportPlatform => PlatformHelper.IsWindows || PlatformHelper.IsLinux;

        public static Task DumpSelfAsync(string outputPath)
        {
            var proc = Process.GetCurrentProcess();
            return DumpAsync(proc, outputPath);
        }
        public static Task DumpAsync(string processName, string outputPath)
        {
            var proc = Process.GetProcessesByName(processName);
            if (proc.Length == 0)
            {
                throw new ArgumentException($"Process {proc} not found");
            }
            return DumpAsync(proc[0], outputPath);
        }
        public static Task DumpAsync(int processId, string outputPath)
        {
            var proc = Process.GetProcessById(processId);
            if (proc == null)
            {
                throw new ArgumentException($"Process {proc} not found");
            }
            return DumpAsync(proc, outputPath);
        }
        public static Task DumpAsync(Process process, string outputPath)
        {
            if (PlatformHelper.IsWindows)
            {
                return windowDumper.CreateDumpAsync(process, outputPath);
            }
            else if (PlatformHelper.IsLinux)
            {
                return linuxDumper.CreateDumpAsync(process, outputPath);
            }
            else
            {
                throw new PlatformNotSupportedException("Can't collect a memory dump on this platform.");
            }
        }
    }
}
