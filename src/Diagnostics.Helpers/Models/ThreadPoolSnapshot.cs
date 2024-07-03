using Microsoft.Diagnostics.Runtime;

namespace Diagnostics.Helpers.Models
{
    public struct ThreadPoolSnapshot
    {
        public int MinThread { get; set; }

        public int MaxThread { get; set; }

        public int IdleWorkerThreads { get; set; }

        public int ActiveWorkerThreads { get; set; }

        public int RetiredWorkerThreads { get; set; }

        public int CpuUtilization { get; set; }

        public int FreeCompletionPorts { get; set; }

        public int TotalCompletionPorts { get; set; }

        public int? TotalThreads => IdleWorkerThreads + ActiveWorkerThreads;

        public override string ToString()
        {
            return string.Format("MinThreads: {0}, MaxThreads: {1}, IdleWorkerThreads: {2}, ActiveWorkerThreads: {3}, RetiredWorkerThreads: {4}, CpuUtilization: {5}, FreeCompletionPorts: {6}, TotalCompletionPorts: {7}",
                MinThread, MaxThread, IdleWorkerThreads, ActiveWorkerThreads, RetiredWorkerThreads, CpuUtilization, FreeCompletionPorts, TotalCompletionPorts);
        }

        public ThreadPoolSnapshot(int minThread, int maxThread, int idleWorkerThreads, int activeWorkerThreads, int retiredWorkerThreads, int cpuUtilization, int freeCompletionPorts, int totalCompletionPorts)
        {
            MinThread = minThread;
            MaxThread = maxThread;
            IdleWorkerThreads = idleWorkerThreads;
            ActiveWorkerThreads = activeWorkerThreads;
            RetiredWorkerThreads = retiredWorkerThreads;
            CpuUtilization = cpuUtilization;
            FreeCompletionPorts = freeCompletionPorts;
            TotalCompletionPorts = totalCompletionPorts;
        }

        public static ThreadPoolSnapshot Create(ClrThreadPool threadPool)
        {
            return new ThreadPoolSnapshot(threadPool.MinThreads,
                threadPool.MaxThreads,
                threadPool.IdleWorkerThreads,
                threadPool.ActiveWorkerThreads,
                threadPool.RetiredWorkerThreads,
                threadPool.CpuUtilization,
                threadPool.FreeCompletionPorts,
                threadPool.TotalCompletionPorts);
        }
    }
}
