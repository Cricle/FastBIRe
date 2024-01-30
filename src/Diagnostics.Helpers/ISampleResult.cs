using System;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public interface ISampleResult : IDisposable
    {
        bool IsStop { get; }

        RuntimeEventCounter Counter { get; }

        ICounterResult CounterResult { get; }

        Task Task { get; }

        void Pause();

        void Resume();

        Task OnceAsync(Action<RuntimeEventCounter> action, CancellationToken token);

        Task<RuntimeEventCounter> OnceAsync(CancellationToken token);
    }
}
