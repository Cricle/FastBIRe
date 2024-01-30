using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Diagnostics.Helpers
{
    public static class StackHelper
    {
        //https://github.com/microsoft/clrmd/blob/main/src/Samples/ClrStack/ClrStack.cs
        public static StackSnapshotCollection GetStackSnapshots()
        {
            var dt = DataTarget.AttachToProcess(PlatformHelper.CurrentProcessId, false);
            return GetStackSnapshots(dt);
        }
        public static StackSnapshotCollection GetStackSnapshots(int processId)
        {
            var dt = DataTarget.AttachToProcess(processId, false);
            return GetStackSnapshots(dt);
        }
        public static StackSnapshotCollection GetStackSnapshots(DataTarget dataTarget)
        {
            var isTarget64Bit = dataTarget.DataReader.PointerSize == 8;
            if (PlatformHelper.Is64Bit != isTarget64Bit)
            {
                throw new Exception(string.Format("Architecture mismatch:  Process is {0} but target is {1}", PlatformHelper.Is64Bit ? "64 bit" : "32 bit", isTarget64Bit ? "64 bit" : "32 bit"));
            }
            var stacks = new List<StackSnapshot>();
            foreach (var version in dataTarget.ClrVersions)
            {
                stacks.Add(new StackSnapshot(version));
            }
            return new StackSnapshotCollection(dataTarget, stacks);
        }
    }
    public record class StackSnapshotCollection : IDisposable
    {
        public StackSnapshotCollection(DataTarget dataTarget, IReadOnlyList<StackSnapshot> stacks)
        {
            DataTarget = dataTarget;
            Stacks = stacks;
        }

        public DataTarget DataTarget { get; }

        public IReadOnlyList<StackSnapshot> Stacks { get; }

        public void Dispose()
        {
            DataTarget.Dispose();
        }
        public override string ToString()
        {
            var s = new StringBuilder();
            foreach (var item in Stacks)
            {
                s.AppendLine(item.ToString());
            }
            return s.ToString();
        }
    }
    public record class StackSnapshot
    {
        public StackSnapshot(ClrInfo clrInfo)
        {
            ClrInfo = clrInfo;
        }

        public ClrInfo ClrInfo { get; }

        public void GetThreadString(StringBuilder builder, ClrRuntime runtime, bool withDos)
        {
            foreach (var item in runtime.Threads)
            {
                if (item.IsAlive)
                {
                    item.GetThreadString(builder, runtime, withDos);
                    builder.AppendLine();
                }
            }
        }
        public override string ToString()
        {
            using (var runtime = ClrInfo.CreateRuntime())
            {
                var s = new StringBuilder();
                s.AppendFormat("CLR: {0}, Thread Count: {1}", ClrInfo, runtime.Threads.Length);
                s.AppendLine();
                GetThreadString(s, runtime, true);
                return s.ToString();
            }
        }
    }
    public static class ClrVisitExtensions
    {
        public static string GetThreadString(this ClrThread thread, ClrRuntime runtime, bool withDos)
        {
            var builder = new StringBuilder();
            GetThreadString(thread, builder, runtime, withDos);
            return builder.ToString();
        }
        public static void GetThreadString(this ClrThread thread, StringBuilder builder, ClrRuntime runtime, bool withDos)
        {
            builder.AppendFormat("Thread {0:X}, LockCount {1}, IsGc {2}, State {3:X}", thread.OSThreadId, thread.LockCount, thread.IsGc, thread.State);
            builder.AppendLine();
            builder.AppendFormat("Stack: {0:X} - {1:X}", thread.StackBase, thread.StackLimit);
            builder.AppendLine();
            GetStackFramesString(thread.EnumerateStackTrace(), builder);
            if (withDos)
            {
                GetDosString(thread, runtime.Heap, runtime.DataTarget, builder);
            }
        }
        public static string GetStackFramesString(this IEnumerable<ClrStackFrame> frames)
        {
            var builder = new StringBuilder();
            GetStackFramesString(frames, builder);
            return builder.ToString();
        }
        public static void GetStackFramesString(this IEnumerable<ClrStackFrame> frames, StringBuilder builder)
        {
            foreach (var frame in frames)
            {
                builder.AppendFormat("{0:x12} {1:x12} {2}", frame.StackPointer, frame.InstructionPointer, frame);
                builder.AppendLine();
            }
        }
        public static void GetDosString(this ClrThread thread, ClrHeap heap, DataTarget target, StringBuilder builder)
        {
            var start = thread.StackBase;
            var stop = thread.StackLimit;

            if (start > stop)
            {
                (stop, start) = (start, stop);
            }

            builder.AppendLine("Stack objects:");

            for (ulong ptr = start; ptr <= stop; ptr += (uint)IntPtr.Size)
            {
                if (!target.DataReader.ReadPointer(ptr, out ulong obj))
                    break;

                var type = heap.GetObjectType(obj);
                if (type == null)
                    continue;

                if (!type.IsFree)
                {
                    builder.AppendFormat("{0,16:X} {1,16:X} {2}", ptr, obj, type.Name);
                    builder.AppendLine();
                }
            }
        }

    }
}