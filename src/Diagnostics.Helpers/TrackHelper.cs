#pragma warning disable CA1416
using Diagnostics.Helpers;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Tracker
{
    public static class FullTrackHelper
    {
        public static readonly IReadOnlyList<MetersIdentity> KnowsMetersIdentities =
        [
            new MetersIdentity(RuntimeEventSampleCreator.Instance, "runtime"),
            new MetersIdentity(KestrelEventSampleCreator.Instance, "kestrel"),
            new MetersIdentity(AspNetCoreHostingEventSampleCreator.Instance, "aspnetcorehosting"),
        ];

        public static async Task WriteArchiveAsync(ZipArchive zip, int processId, bool withDump = false, int? withTrace = null, TextWriter? logWriter = null)
        {
            logWriter?.WriteLine("Exporting processinfo");
            var procInfoEntity = zip.CreateEntry(".procinfo");
            using (var stream = procInfoEntity.Open())
            using (var writer = new StreamWriter(stream))
            {
                await WriteProcessInfoAsync(writer, processId);
            }
            //var systemStatsEntity = zip.CreateEntry(".systemstats");
            //using (var stream = systemStatsEntity.Open())
            //using (var writer = new StreamWriter(stream))
            //{
            //    await WriteSystemStatusAsync(writer);
            //}
            var deviceEntity = zip.CreateEntry(".device");
            using (var stream = deviceEntity.Open())
            using (var writer = new StreamWriter(stream))
            {
                await WriteDriveInfosync(writer);
            }
            logWriter?.WriteLine("Exporting gcdumpinfo");
            var gcdumpInfoEntity = zip.CreateEntry(".gcdumpinfo");
            using (var gcdumpinfo = gcdumpInfoEntity.Open())
            using (var gcdumpinfoTw = new StreamWriter(gcdumpinfo))
            {
                await WriteGcDumpAsync(gcdumpinfoTw, processId);
            }
            if (withDump)
            {
                logWriter?.WriteLine("Exporting dump");
                var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".dump");
                Dump(processId, tmp);
                zip.CreateEntryFromFile(tmp, ".dump");
            }
            logWriter?.WriteLine("Exporting stack");
            var stackEntity = zip.CreateEntry(".stack");
            using (var stackStream = stackEntity.Open())
            using (var stackStreamWriter = new StreamWriter(stackStream))
            {
                await WriteStackAsync(stackStreamWriter, processId);
            }

            logWriter?.WriteLine("Exporting Meters");
            var delayTime = TimeSpan.FromSeconds(2);
            await WriteMetersAsync(processId, KnowsMetersIdentities, delayTime);
            if (withTrace != null && withTrace != 0)
            {
                logWriter?.WriteLine($"Collecting nettrace with {withTrace} seconds");
                var nettraceEntity = zip.CreateEntry(".nettrace");
                using (var nettraceStream = nettraceEntity.Open())
                {
                    await WriteTraceAsync(nettraceStream, processId, TimeSpan.FromSeconds(withTrace!.Value));
                }
            }
        }

        public static async Task WriteProcessInfoAsync(TextWriter writer, int processId)
        {
            var proc = Process.GetProcessById(processId);
            await writer.WriteLineAsync("Name:                  " + proc.ProcessName);
            await writer.WriteLineAsync("VirtualMemorySize64:   " + proc.VirtualMemorySize64 + $"({FormatBytes(proc.VirtualMemorySize64)})");
            await writer.WriteLineAsync("WorkingSet64:          " + proc.WorkingSet64 + $"({FormatBytes(proc.WorkingSet64)})");
            await writer.WriteLineAsync("UserProcessorTime:     " + proc.UserProcessorTime);
            await writer.WriteLineAsync("TotalProcessorTime:    " + proc.TotalProcessorTime);
            await writer.WriteLineAsync("Threads Count:         " + proc.Threads.Count);
            for (int i = 0; i < proc.Threads.Count; i++)
            {
                var thread = proc.Threads[i];
                await writer.WriteLineAsync($"\t Id: 0x{thread.Id:X} ThreadState: {thread.ThreadState} WaitReason: {(thread.ThreadState == System.Diagnostics.ThreadState.Wait ? thread.WaitReason.ToString() : string.Empty)}");
            }
        }
        public static async Task WriteDriveInfosync(TextWriter writer)
        {
            var driveInfos = DriveInfo.GetDrives();
            for (int i = 0; i < driveInfos.Length; i++)
            {
                var driverInfo = driveInfos[i];
                await writer.WriteLineAsync(driverInfo.Name);
                await writer.WriteLineAsync($"\t DriveType:            {driverInfo.DriveType}");
                await writer.WriteLineAsync($"\t IsReady:              {driverInfo.IsReady}");
                await writer.WriteLineAsync($"\t DriveFormat:          {driverInfo.DriveFormat}");
                await writer.WriteLineAsync($"\t VolumeLabel:          {driverInfo.VolumeLabel}");
                await writer.WriteLineAsync($"\t TotalSize:            {driverInfo.TotalSize}({FormatBytes(driverInfo.TotalSize)})");
                await writer.WriteLineAsync($"\t AvailableFreeSpace:   {driverInfo.AvailableFreeSpace}({FormatBytes(driverInfo.AvailableFreeSpace)})");
                await writer.WriteLineAsync($"\t TotalFreeSpace:       {driverInfo.TotalFreeSpace}({FormatBytes(driverInfo.TotalFreeSpace)})");
            }
        }
        public static async Task WriteGcDumpAsync(TextWriter writer, int processId)
        {
            GcDumpHelper.TryCollectMemoryGraph(processId, 10_000, null, out var mg, default);
            await GcDumpHelper.WriteAsync(mg, writer);
        }
        public static Task WriteStackAsync(TextWriter writer, int processId)
        {
            var ss = StackHelper.GetStackSnapshots(processId);
            return writer.WriteAsync(ss.ToString());
        }
        public static async Task<IList<MetersResult>> WriteMetersAsync(int processId, IEnumerable<MetersIdentity> meters, TimeSpan delayTime)
        {
            var results = new List<MetersResult>();
            foreach (var item in meters)
            {
                await CreateMetersAsync(processId, item, delayTime);
            }
            return results;
        }
        public static void Dump(int processId, string path)
        {
            DumpHelper.Dump(processId, path);
        }
        public static async Task WriteTraceAsync(Stream stream, int processId, TimeSpan timeout)
        {
            var token = new CancellationTokenSource(timeout);
            var providers = new List<EventPipeProvider>();
            providers.AddRange(WellKnowsEventProvider.CpuSamplingProviders);
            providers.AddRange(WellKnowsEventProvider.DatabaseProviders);
            providers.AddRange(WellKnowsEventProvider.GCCollectProviders);
            providers.AddRange(WellKnowsEventProvider.GCVerboseProviders);
            try
            {
                await TraceHelper.TraceAsync(processId, providers, stream, token: token.Token);
            }
            catch (OperationCanceledException)
            {

            }
        }
        private static async Task<MetersResult> CreateMetersAsync(int processId, MetersIdentity identity, TimeSpan delayTime)
        {
            var meters = EventSampleCreatorCreateHelper.GetIntervalSample(identity.EventSampleCreator, processId, TimeSpan.FromSeconds(1));
            await Task.Delay(delayTime);
            return new MetersResult(identity, meters.Counter.ToString() ?? string.Empty);
        }
        public static unsafe string GetSystemInfo()
        {
            try
            {

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    Windows.Win32.System.SystemInformation.MEMORYSTATUSEX mem = default;
                    mem.dwLength = (uint)Marshal.SizeOf(mem);
                    Windows.Win32.System.SystemInformation.MEMORYSTATUSEX* ptr = (Windows.Win32.System.SystemInformation.MEMORYSTATUSEX*)Unsafe.AsPointer(ref mem);
                    var res = Windows.Win32.PInvoke.GlobalMemoryStatusEx(ptr);
                    using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
                    var procName = key.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0");
                    var val = procName?.GetValue("ProcessorNameString")?.ToString()?.Trim();
                    return $"{procName}, {(mem.ullTotalVirtual / 1024 / 1024.0):f5}Gb";
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    string cpuInfoPath = "/proc/cpuinfo";
                    if (File.Exists(cpuInfoPath))
                    {
                        string[] lines = File.ReadAllLines(cpuInfoPath);
                        string? modelName = null;
                        ulong physicalMem = 0;
                        using (var reader = new StreamReader("/proc/meminfo"))
                        {
                            string? line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (line.StartsWith("MemTotal:"))
                                {
                                    string[] parts = line.Split(':');
                                    if (parts.Length == 2)
                                    {
                                        string[] sizeParts = parts[1].Trim().Split(' ');
                                        if (sizeParts.Length >= 1 && sizeParts[0].Length > 0)
                                        {
                                            physicalMem = Convert.ToUInt64(sizeParts[0]);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        foreach (string line in lines)
                        {
                            if (line.StartsWith("model name"))
                            {
                                modelName = line.Split(':')[1].Trim();
                            }
                            if (string.IsNullOrWhiteSpace(line))
                            {
                                break;
                            }
                        }
                        return $"{modelName}, {(physicalMem / 1024 / 1024.0):f5}Gb";
                    }
                }
                return "Unknow";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ex.Message;
            }
        }

        private static readonly string[] suffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
        private static string FormatBytes(long bytes)
        {
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:N1} {suffixes[suffixIndex]}";
        }
    }
}
