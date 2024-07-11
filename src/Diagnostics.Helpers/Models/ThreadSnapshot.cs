using Microsoft.Diagnostics.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnostics.Helpers.Models
{
    public struct ThreadSnapshot
    {
        public ThreadSnapshot(uint oSThreadId, uint lockCount, bool isGc, ClrThreadState state, bool isFinalizer, ulong stackBase, ulong stackLimit, bool isThreadPool, IReadOnlyList<ThreadStackFrame> stackFrames)
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

        public uint OSThreadId { get; set; }

        public uint LockCount { get; set; }

        public bool IsGc { get; set; }

        public ClrThreadState State { get; set; }

        public bool IsFinalizer { get; set; }

        public ulong StackBase { get; set; }

        public ulong StackLimit { get; set; }

        public bool IsThreadPool { get; set; }

        public IReadOnlyList<ThreadStackFrame>? StackFrames { get; set; }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.AppendFormat("Thread {0:X}, LockCount:{1}, IsGc:{2}, State:{3:X} IsFinalizer:{4}", OSThreadId, LockCount, IsGc, State, IsFinalizer);
            s.AppendFormat("Stack: {0:X} - {1:X}", StackBase, StackLimit);
            if (StackFrames != null)
            {
                foreach (var item in StackFrames)
                {
                    s.AppendLine(item.ToString());
                }
            }
            s.AppendLine();
            return s.ToString();
        }

        public static ThreadSnapshot Create(ClrThread thread)
        {
            var frames = new List<ThreadStackFrame>();
            foreach (var item in thread.EnumerateStackTrace())
            {
                frames.Add(ThreadStackFrame.Create(item));
            }
            return new ThreadSnapshot(
                thread.OSThreadId,
                thread.LockCount,
                thread.IsGc,
                thread.State,
                thread.IsFinalizer,
                thread.StackBase,
                thread.StackLimit,
                thread.State== ClrThreadState.TS_TPWorkerThread,
                frames);
        }
    }
}
