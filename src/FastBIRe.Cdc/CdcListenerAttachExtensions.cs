using FastBIRe.Cdc.Events;
using System;

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
}
