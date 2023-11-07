using FastBIRe.Cdc.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc
{
    public static class CdcListenerAttachExtensions
    {
        class AttackResult : IDisposable
        {
            public readonly ICdcListener Listener;
            public readonly IEventDispatcher<CdcEventArgs> Dispatcher;

            public AttackResult(ICdcListener listener, IEventDispatcher<CdcEventArgs> dispatcher)
            {
                Listener = listener;
                Dispatcher = dispatcher;
                Listener.EventRaised += OnListenerEventRaised;
            }

            private void OnListenerEventRaised(object? sender, CdcEventArgs e)
            {
                Dispatcher.Add(e);
            }

            public void Dispose()
            {
                Listener.EventRaised -= OnListenerEventRaised;
            }
        }
        public static IDisposable AttachToDispatcher(this ICdcListener listener, IEventDispatcher<CdcEventArgs> dispatcher)
        {
            return new AttackResult(listener, dispatcher);
        }

    }
    public interface ICdcListener : IDisposable
    {
        event EventHandler<CdcEventArgs>? EventRaised;

        event EventHandler<CdcErrorEventArgs>? Error;

        IGetCdcListenerOptions Options { get; }

        bool IsStarted { get; }

        Task StopAsync(CancellationToken token = default);

        Task StartAsync(CancellationToken token = default);

        ITableMapInfo? GetTableMapInfo(object id);
    }
}
