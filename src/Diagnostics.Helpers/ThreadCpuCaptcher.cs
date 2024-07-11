using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Diagnostics.Helpers
{
    public class ThreadCpuCaptcher
    {
        class ThreadUserTime : IThreadUserTime
        {
            public TimeSpan lastTotalTime;

            public TimeSpan LastTotalTime => lastTotalTime;

            public double lastCpuUsaged;

            public double LastCpuUsaged => lastCpuUsaged;
        }
        public ThreadCpuCaptcher(Process process)
        {
            Process = process;
            process.Exited += OnExited;
            lastUserTime = new Dictionary<int, IThreadUserTime>();
        }
        private long lastCaptchTime;
        private TimeSpan lastProcessorTime;
        private double currentCPUUsaged;
        private readonly Dictionary<int, IThreadUserTime> lastUserTime;

        public double CurrentCPUUsaged => currentCPUUsaged;

        public IReadOnlyDictionary<int, IThreadUserTime> LastUserTime => lastUserTime;

        public bool IsFirst => lastCaptchTime == 0;

        public void Update()
        {
            Process.Refresh();
            if (Process.HasExited)
            {
                return;
            }
            if (lastCaptchTime != 0)
            {
                var subTime = new TimeSpan(Stopwatch.GetTimestamp() - lastCaptchTime).TotalMilliseconds;
                var notHitThreadIds = new HashSet<int>(lastUserTime.Keys);
                currentCPUUsaged = ((Process.UserProcessorTime - lastProcessorTime).TotalMilliseconds / subTime) * 100;
                if (currentCPUUsaged>100)
                {
                    currentCPUUsaged = -1;
                }
                foreach (ProcessThread item in Process.Threads)
                {
                    if (item.ThreadState == ThreadState.Terminated)
                    {
                        continue;
                    }
                    notHitThreadIds.Remove(item.Id);
                    if (lastUserTime.TryGetValue(item.Id, out var old))
                    {
                        try
                        {
                            var sub = (item.UserProcessorTime - old.LastTotalTime).TotalMilliseconds / subTime * 100;
                            ((ThreadUserTime)old).lastTotalTime = item.UserProcessorTime;
                            if (sub <= 100)
                            {
                                ((ThreadUserTime)old).lastCpuUsaged = sub;
                            }
                            else
                            {
                                ((ThreadUserTime)old).lastCpuUsaged = -1;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        lastUserTime[item.Id] = new ThreadUserTime
                        {
                            lastTotalTime = item.UserProcessorTime
                        };
                        continue;
                    }
                }
                if (notHitThreadIds.Count != 0)
                {
                    foreach (var h in notHitThreadIds)
                    {
                        lastUserTime.Remove(h);
                    }
                }
                lastProcessorTime = Process.UserProcessorTime;
            }
            else
            {
                foreach (ProcessThread item in Process.Threads)
                {
                    lastUserTime[item.Id] = new ThreadUserTime { lastTotalTime = item.UserProcessorTime };
                }
            }
            lastCaptchTime = Stopwatch.GetTimestamp();
        }

        private void OnExited(object? sender, EventArgs e)
        {

        }

        public Process Process { get; }
    }
}
