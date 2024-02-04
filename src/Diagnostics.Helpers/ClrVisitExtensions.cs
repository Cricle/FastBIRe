using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Diagnostics.Helpers
{
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