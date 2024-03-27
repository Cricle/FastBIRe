using System;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public interface ISampleProvider : IDisposable
    {
        bool IsStop { get; }

        IEventCounterProvider Counter { get; }

        ICounterResult CounterResult { get; }

        Task Task { get; }

        void Pause();

        void Resume();

        Task OnceAsync(CancellationToken token);
    }
}
