using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.Events
{
    public interface IEventDispatcher<TInput> : IDisposable
    {
        bool IsStarted { get; }

        Task StartAsync(CancellationToken token = default);

        Task StopAsync(CancellationToken token = default);

        void Add(TInput args);

        void AddRange(IEnumerable<TInput> args);

        Task HandleAsync(TInput eventArgs, CancellationToken cancellationToken = default);
    }
}
