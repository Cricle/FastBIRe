using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Generator.Core
{

    public static class EventSourceDiagnostic
    {
        public static Task<EventWrittenEventArgs> GetOnceAsync<TEventSource>(TEventSource eventSource, EventLevel eventLevel = EventLevel.LogAlways, EventKeywords matchAnyKeyword = EventKeywords.None, IDictionary<string, string> arguments = null, CancellationToken token = default)
            where TEventSource : EventSource
        {
            var taskSource = new TaskCompletionSource<EventWrittenEventArgs>();
            token.Register(() => taskSource.TrySetCanceled());
            var listener = new OnceEventListener<TEventSource>((arg) => taskSource.TrySetResult(arg));
            listener.EnableEvents(eventSource, eventLevel, matchAnyKeyword, arguments);
            return taskSource.Task;
        }

        internal class OnceEventListener<TEventSource> : EventListener
        {
            public OnceEventListener(Action<EventWrittenEventArgs> onceRaised)
            {
                OnceRaised = onceRaised;
            }

            public Action<EventWrittenEventArgs> OnceRaised { get; }

            protected override void OnEventSourceCreated(EventSource eventSource)
            {
                if (eventSource is TEventSource)
                {
                    base.OnEventSourceCreated(eventSource);
                }
            }
            protected override void OnEventWritten(EventWrittenEventArgs eventData)
            {
                if (eventData.EventSource is TEventSource)
                {
                    OnceRaised(eventData);
                    Dispose();
                }
            }
        }
    }

}
