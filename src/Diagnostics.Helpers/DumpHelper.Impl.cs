using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public static partial class DumpHelper
    {
        internal interface IMemoryDumper
        {
            Task CreateDumpAsync(Process process,string outputPath);
        }
        /// <summary>
        /// From https://github.com/aspnet/AspLabs/blob/master/src/DotNetDiagnostics/src/dotnet-dump/Dumper.Linux.cs
        /// </summary>
        internal class LinuxMemoryDumper : IMemoryDumper
        {
            public Task CreateDumpAsync(Process process, string outputPath)
            {
                return Linux.CollectDumpAsync(process, outputPath);
            }

            private static class Linux
            {
                internal static async Task CollectDumpAsync(Process process, string fileName)
                {
                    // We don't work on WSL :(
                    var ostype = File.ReadAllText("/proc/sys/kernel/osrelease");
                    if (ostype.Contains("Microsoft"))
                    {
                        throw new PlatformNotSupportedException("Cannot collect memory dumps from Windows Subsystem for Linux.");
                    }

                    // First step is to find the .NET runtime. To do this we look for coreclr.so
                    var coreclr = process.Modules.Cast<ProcessModule>().FirstOrDefault(m => string.Equals(m.ModuleName, "libcoreclr.so"));
                    if (coreclr == null)
                    {
                        throw new NotSupportedException("Unable to locate .NET runtime associated with this process!");
                    }

                    // Find createdump next to that file
                    var runtimeDirectory = Path.GetDirectoryName(coreclr.FileName);
                    var createDumpPath = Path.Combine(runtimeDirectory, "createdump");
                    if (!File.Exists(createDumpPath))
                    {
                        throw new NotSupportedException($"Unable to locate 'createdump' tool in '{runtimeDirectory}'");
                    }

                    // Create the dump
                    var exitCode = await CreateDumpAsync(createDumpPath, fileName, process.Id);
                    if (exitCode != 0)
                    {
                        throw new Exception($"createdump exited with non-zero exit code: {exitCode}");
                    }
                }

                private static Task<int> CreateDumpAsync(string exePath, string fileName, int processId)
                {
                    var tcs = new TaskCompletionSource<int>();
                    var createdump = new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = exePath,
                            Arguments = $"-f {fileName} {processId}",
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            RedirectStandardInput = true,
                        },
                        EnableRaisingEvents = true,
                    };
                    createdump.Exited += (s, a) => tcs.TrySetResult(createdump.ExitCode);
                    createdump.Start();
                    return tcs.Task;
                }
            }
        }
        /// <summary>
        /// From https://github.com/aspnet/AspLabs/blob/master/src/DotNetDiagnostics/src/dotnet-dump/Dumper.Windows.cs
        /// </summary>
        internal class WindowsMemoryDumper : IMemoryDumper
        {
            public Task CreateDumpAsync(Process process, string outputPath)
            {
                using (var fs = File.Open(outputPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    // Dump the process!
                    var exceptionInfo = new NativeMethods.MINIDUMP_EXCEPTION_INFORMATION();
                    if (!NativeMethods.MiniDumpWriteDump(process.Handle,
                        (uint)process.Id,
                        fs.SafeFileHandle,
                        NativeMethods.MINIDUMP_TYPE.MiniDumpWithFullMemory,
                        ref exceptionInfo,
                        IntPtr.Zero,
                        IntPtr.Zero))
                    {
                        var err = Marshal.GetHRForLastWin32Error();
                        Marshal.ThrowExceptionForHR(err);
                    }
                }
                return Task.CompletedTask;
            }


            private static class NativeMethods
            {
                [DllImport("Dbghelp.dll")]
                public static extern bool MiniDumpWriteDump(IntPtr hProcess, uint ProcessId, SafeFileHandle hFile, MINIDUMP_TYPE DumpType, ref MINIDUMP_EXCEPTION_INFORMATION ExceptionParam, IntPtr UserStreamParam, IntPtr CallbackParam);

                [StructLayout(LayoutKind.Sequential, Pack = 4)]
                public struct MINIDUMP_EXCEPTION_INFORMATION
                {
                    public uint ThreadId;
                    public IntPtr ExceptionPointers;
                    public int ClientPointers;
                }

                [Flags]
                public enum MINIDUMP_TYPE : uint
                {
                    MiniDumpNormal = 0,
                    MiniDumpWithDataSegs = 1 << 0,
                    MiniDumpWithFullMemory = 1 << 1,
                    MiniDumpWithHandleData = 1 << 2,
                    MiniDumpFilterMemory = 1 << 3,
                    MiniDumpScanMemory = 1 << 4,
                    MiniDumpWithUnloadedModules = 1 << 5,
                    MiniDumpWithIndirectlyReferencedMemory = 1 << 6,
                    MiniDumpFilterModulePaths = 1 << 7,
                    MiniDumpWithProcessThreadData = 1 << 8,
                    MiniDumpWithPrivateReadWriteMemory = 1 << 9,
                    MiniDumpWithoutOptionalData = 1 << 10,
                    MiniDumpWithFullMemoryInfo = 1 << 11,
                    MiniDumpWithThreadInfo = 1 << 12,
                    MiniDumpWithCodeSegs = 1 << 13,
                    MiniDumpWithoutAuxiliaryState = 1 << 14,
                    MiniDumpWithFullAuxiliaryState = 1 << 15,
                    MiniDumpWithPrivateWriteCopyMemory = 1 << 16,
                    MiniDumpIgnoreInaccessibleMemory = 1 << 17,
                    MiniDumpWithTokenInformation = 1 << 18,
                    MiniDumpWithModuleHeaders = 1 << 19,
                    MiniDumpFilterTriage = 1 << 20,
                    MiniDumpWithAvxXStateContext = 1 << 21,
                    MiniDumpWithIptTrace = 1 << 22,
                    MiniDumpValidTypeFlags = (-1) ^ ((~1) << 22)
                }
            }
        }
    }
}
