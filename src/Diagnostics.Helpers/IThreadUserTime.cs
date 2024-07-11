#pragma warning disable CA1416

#pragma warning disable CA1416
using System;

namespace Diagnostics.Helpers
{
    public interface IThreadUserTime
    {
        public TimeSpan LastTotalTime { get; }

        public double LastCpuUsaged { get; }
    }
}
