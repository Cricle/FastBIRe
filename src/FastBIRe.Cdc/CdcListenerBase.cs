using FastBIRe.Cdc.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc
{
    public abstract class CdcListenerBase : DisposeObject,ICdcListener
    {
        private CancellationTokenSource? tokenSource;
        private int isStarted;

        public bool IsStarted => Volatile.Read(ref isStarted) != 0;

        protected CancellationTokenSource? TokenSource => tokenSource;

        public event EventHandler<CdcEventArgs>? EventRaised;

        protected void RaiseEvent(CdcEventArgs e)
        {
            EventRaised?.Invoke(this, e);
        }

        public async Task StartAsync(CancellationToken token=default)
        {
            if (IsStarted)
            {
                await StopAsync(token);
            }
            tokenSource = new CancellationTokenSource();
            await OnStartAsync(token);
        }

        protected abstract Task OnStartAsync(CancellationToken token = default);

        public async Task StopAsync(CancellationToken token = default)
        {
            if (IsStarted)
            {
                tokenSource?.Cancel();
                tokenSource?.Dispose();
                await OnStopAsync(token);
            }
        }
        protected abstract Task OnStopAsync(CancellationToken token = default);
        public abstract ITableMapInfo? GetTableMapInfo(object id);
    }
}
