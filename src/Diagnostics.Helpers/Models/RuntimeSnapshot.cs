using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnostics.Helpers.Models
{
    public struct RuntimeSnapshot
    {
        public RuntimeSnapshot(string? fileName, Version? version, IList<ThreadSnapshot> threads, ThreadPoolSnapshot? threadPool)
        {
            Time = DateTime.Now;
            FileName = fileName;
            Version = version;
            Threads = threads;
            ThreadPool = threadPool;
        }

        public DateTime Time { get; }

        public string? FileName { get; set; }

        public Version? Version { get; set; }

        public ThreadPoolSnapshot? ThreadPool { get; set; }

        public IList<ThreadSnapshot> Threads { get; set; }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.AppendFormat("File:{0}, Version:{1}", FileName, Version);
            if (ThreadPool != null)
            {
                s.AppendLine(ThreadPool.ToString());
            }
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
            var threads = new ThreadSnapshot[runtime.Threads.Length];
            for (int i = 0; i < runtime.Threads.Length; i++)
            {
                var thread = runtime.Threads[i];
                threads[i] = ThreadSnapshot.Create(thread);
            }
            ThreadPoolSnapshot? threadPoolSnapshot = null;
            if (threadPool != null)
            {
                threadPoolSnapshot = ThreadPoolSnapshot.Create(threadPool);
            }
            return new RuntimeSnapshot(module.FileName, module.Version, threads, threadPoolSnapshot);
        }
    }
}
