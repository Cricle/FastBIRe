using System;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public class SampleResult<TCounter> : SampleProvider, ISampleResult<TCounter>
        where TCounter : IEventCounter<TCounter>
    {
        public new TCounter Counter { get; }

        IEventCounterProvider ISampleProvider.Counter => Counter;

        public SampleResult(ICounterResult counterResult, TCounter counter)
            : base(counterResult,counter)
        {
            Counter = counter;
        }

        protected override void OnPayloadReceived(object? sender, ICounterPayload e)
        {
            Counter.Update(e);
        }
        protected override void OnDisposed()
        {
            if (Counter is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        protected override Task GetOnceTask(CancellationToken token)
        {
            return Counter.OnceAsync(token);
        }
    }
}
