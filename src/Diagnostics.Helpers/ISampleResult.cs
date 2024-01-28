using System;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public interface ISampleResult : IDisposable
    {
        RuntimeEventCounter Counter { get; }

        ICounterResult CounterResult { get; }

        Task StartAsync(CancellationToken token);
    }
}
