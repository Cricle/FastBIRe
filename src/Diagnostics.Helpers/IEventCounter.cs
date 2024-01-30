using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public interface IEventCounter<TCounter>
    {
        bool AllNotNull { get; }

        event EventHandler? Changed;

        TCounter Copy();
        Task<TCounter> OnceAsync(CancellationToken token = default);
        Task OnceAsync(Action<TCounter> action, CancellationToken token = default);
        void Reset();
        void Update(ICounterPayload payload);
        void WriteTo(TextWriter sb);
    }
}