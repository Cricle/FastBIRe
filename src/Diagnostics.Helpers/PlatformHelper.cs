using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Diagnostics.Helpers
{
    public static class PlatformHelper
    {
        public static bool IsWindows { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsLinux { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static bool Is64Bit { get; } = Environment.Is64BitProcess;

        public static int CurrentProcessId { get; } =
#if NETSTANDARD2_0
            Process.GetCurrentProcess().Id
#else
            Environment.ProcessId
#endif
            ;
    }
}
