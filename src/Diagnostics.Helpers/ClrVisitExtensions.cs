using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnostics.Helpers
{
    public readonly record struct ThreadStackFrame
    {
        public ThreadStackFrame(ulong stackPointer, ulong instructionPointer, string stackTrace)
        {
            StackPointer = stackPointer;
            InstructionPointer = instructionPointer;
            StackTrace = stackTrace;
        }

        public ulong StackPointer { get; }

        public ulong InstructionPointer { get; }

        public string StackTrace { get; }

        public override string ToString()
        {
            return string.Format("{0:x12} {1:x12} {2}", StackPointer, InstructionPointer, StackTrace);
        }
        public static ThreadStackFrame Create(ClrStackFrame frame)
        {
            return new ThreadStackFrame(frame.StackPointer, frame.InstructionPointer, frame.ToString()??string.Empty);
        }
    }
    public readonly record struct RuntimeSnapshot
    {
        public RuntimeSnapshot(string fileName, Version version, int? threadPoolMinThread, int? threadPoolMaxThread, int? threadPoolIdleWorkerThreads,int? threadPoolActiveWorkerThreads, IList<ThreadSnapshot> threads)
        {
            FileName = fileName;
            Version = version;
            ThreadPoolMinThread = threadPoolMinThread;
            ThreadPoolMaxThread = threadPoolMaxThread;
            ThreadPoolIdleWorkerThreads = threadPoolIdleWorkerThreads;
            Threads = threads;
            ThreadPoolActiveWorkerThreads = threadPoolActiveWorkerThreads;
        }

        public string FileName { get; }

        public Version Version { get; }

        public int? ThreadPoolMinThread { get; }

        public int? ThreadPoolMaxThread { get; }

        public int? ThreadPoolActiveWorkerThreads { get; }

        public int? ThreadPoolIdleWorkerThreads { get; }

        public int? ThreadPoolTotalThreads => ThreadPoolIdleWorkerThreads + ThreadPoolActiveWorkerThreads;

        public IList<ThreadSnapshot> Threads { get; }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.AppendFormat("File:{0}, Version:{1}, ThreadPoolMin:{2}, ThreadPoolMax:{3}, ThreadPoolIdle:{4}, ThreadPoolActive:{5}, ThreadPoolTotal:{6}", FileName, Version, ThreadPoolMinThread, ThreadPoolMaxThread, ThreadPoolIdleWorkerThreads, ThreadPoolActiveWorkerThreads, ThreadPoolTotalThreads);
            s.AppendLine();
            foreach (var item in Threads)
            {
                s.AppendLine(item.ToString());
            }
            return s.ToString();
        }
        public static RuntimeSnapshot Create(ClrRuntime runtime)
        {
            var module = runtime.ClrInfo.ModuleInfo;
            var threadPool = runtime.ThreadPool;
            var threadInfos = runtime.Threads.Select(ThreadSnapshot.Create).ToList();
            return new RuntimeSnapshot(module.FileName, module.Version, threadPool?.MinThreads, threadPool?.MaxThreads, threadPool?.IdleWorkerThreads,threadPool?.ActiveWorkerThreads, threadInfos);
        }
    }
    public readonly record struct ThreadSnapshot
    {
        public ThreadSnapshot(uint oSThreadId, uint lockCount, bool isGc, ClrThreadState state, bool isFinalizer, ulong stackBase, ulong stackLimit,bool isThreadPool, IReadOnlyList<ThreadStackFrame> stackFrames)
        {
            OSThreadId = oSThreadId;
            LockCount = lockCount;
            IsGc = isGc;
            State = state;
            IsFinalizer = isFinalizer;
            StackBase = stackBase;
            StackLimit = stackLimit;
            StackFrames = stackFrames;
            IsThreadPool = isThreadPool;
        }

        public uint OSThreadId { get; }

        public uint LockCount { get; }

        public bool IsGc { get; }

        public ClrThreadState State { get; }

        public bool IsFinalizer { get; }

        public ulong StackBase { get; }

        public ulong StackLimit { get; }

        public bool IsThreadPool { get; }

        public IReadOnlyList<ThreadStackFrame> StackFrames { get; }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.AppendFormat("Thread {0:X}, LockCount:{1}, IsGc:{2}, State:{3:X} IsFinalizer:{4}", OSThreadId, LockCount,IsGc, State, IsFinalizer);
            s.AppendFormat("Stack: {0:X} - {1:X}", StackBase, StackLimit);
            foreach (var item in StackFrames)
            {
                s.AppendLine(item.ToString());
            }
            s.AppendLine();
            return s.ToString();
        }

        public static ThreadSnapshot Create(ClrThread thread)
        {
            var frames = thread.EnumerateStackTrace().Select(x=>ThreadStackFrame.Create(x)).ToList();
            return new ThreadSnapshot(
                thread.OSThreadId,
                thread.LockCount,
                thread.IsGc,
                thread.State,
                thread.IsFinalizer,
                thread.StackBase,
                thread.StackLimit,
                false,
                frames);
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
        public static ThreadSnapshot GetThreadSnapshot(this ClrThread thread)
        {
            return ThreadSnapshot.Create(thread);
        }
        public static void GetThreadString(this ClrThread thread, StringBuilder builder, ClrRuntime runtime, bool withDos)
        {
            builder.AppendFormat("Thread {0:X}, LockCount:{1}, IsGc:{2}, State:{3:X} IsFinalizer:{4}", thread.OSThreadId, thread.LockCount, thread.IsGc, thread.State, thread.IsFinalizer);
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