using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public static class CounterHelper
    {
        public static ICounterResult CreateCounter(int processId, IEnumerable<EventPipeProvider> providers, CounterConfiguration? configuration = null, bool requestRundown = true, int bufferSizeInMB = 256, int defaultIntervalSeconds = 1)
        {
            configuration ??= new CounterConfiguration(CounterFilter.AllCounters(defaultIntervalSeconds));
            var client = new DiagnosticsClient(processId);
            return new CounterResult(processId, providers, configuration, requestRundown, bufferSizeInMB, client);
        }
        public static ICounterResult CreateCounter(Func<EventSource, bool>? isAccept,int intervalSecond=1)
        {
            return new InProcessCounterResult(isAccept, intervalSecond);
        }
    }
    public readonly record struct ExceptionReceivedPlayload
    {
        internal ExceptionReceivedPlayload(TraceEvent? traceEvent, Exception exception, EventWrittenEventArgs? eventArgs)
        {
            TraceEvent = traceEvent;
            Exception = exception;
            EventArgs = eventArgs;
        }

        public TraceEvent? TraceEvent { get; }

        public EventWrittenEventArgs? EventArgs { get; }

        public Exception Exception { get; }
    }
    public interface ICounterResult
    {
        event EventHandler<ICounterPayload>? PayloadReceived;

        event EventHandler<ExceptionReceivedPlayload>? ExceptionReceived;

        Task StartSessionAsync(CancellationToken token);
    }
    internal class InProcessCounterResult : EventListener, ICounterResult
    {
        public event EventHandler<ICounterPayload>? PayloadReceived;
        public event EventHandler<ExceptionReceivedPlayload>? ExceptionReceived;

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            try
            {
                if (eventData.TryGetCounterPayload(out var payload) && payload != null)
                {
                    PayloadReceived?.Invoke(this, payload);
                }
            }
            catch (Exception ex)
            {
                ExceptionReceived?.Invoke(this, new ExceptionReceivedPlayload(null, ex, eventData));
            }
        }
        private readonly int intervalSecond;

        public Func<EventSource, bool>? IsAccept { get; }

        public InProcessCounterResult(Func<EventSource, bool>? isAccept, int intervalSecond)
        {
            IsAccept = isAccept;
            if (intervalSecond<=0)
            {
                throw new ArgumentException("The intervalSecond must more than zero");
            }
            this.intervalSecond = intervalSecond;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsActualAccept(EventSource source)
        {
            return IsAccept == null || IsAccept(source);
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (IsActualAccept(eventSource))
            {
                var sec = intervalSecond;
                if (sec <= 0)
                {
                    sec = 1;
                }
                base.OnEventSourceCreated(eventSource);
                EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All,new Dictionary<string, string?>(1) 
                {
                    ["EventCounterIntervalSec"] = sec.ToString()
                });
            }
        }

        public Task StartSessionAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
    internal class CounterResult: ICounterResult
    {
        public CounterResult(int processId, IEnumerable<EventPipeProvider> providers, CounterConfiguration configuration, bool requestRundown, int bufferSizeInMB, DiagnosticsClient client)
        {
            ProcessId = processId;
            Providers = providers;
            Configuration = configuration;
            RequestRundown = requestRundown;
            BufferSizeInMB = bufferSizeInMB;
            Client = client;
        }

        public int ProcessId { get; }

        public IEnumerable<EventPipeProvider> Providers { get; }

        public CounterConfiguration Configuration { get; }

        public bool RequestRundown { get; }

        public int BufferSizeInMB { get; }

        public DiagnosticsClient Client { get; }

        public event EventHandler<ICounterPayload>? PayloadReceived;
        public event EventHandler<ExceptionReceivedPlayload>? ExceptionReceived;

        public async Task StartSessionAsync(CancellationToken token)
        {
            var taskSource= new TaskCompletionSource<bool>();
            using (var session = Client.StartEventPipeSession(Providers, RequestRundown, BufferSizeInMB))
            using (var source = new EventPipeEventSource(session.EventStream))
            {
                token.Register(() =>
                {
                    taskSource.SetResult(true);
                    source.StopProcessing();
                    session.Stop();
                });
                if (RequestRundown)
                {
                    Client.ResumeRuntime();
                }
                source.Dynamic.All += (traceEvent) =>
                {
                    if (PayloadReceived != null)
                    {
                        try
                        {
                            if (traceEvent.TryGetCounterPayload(Configuration, out ICounterPayload counterPayload))
                            {
                                PayloadReceived?.Invoke(this, counterPayload);
                            }
                        }
                        catch (Exception ex)
                        {
                            ExceptionReceived?.Invoke(this, new ExceptionReceivedPlayload(traceEvent, ex,null));
                        }
                    }
                };

                source.Process();
                await taskSource.Task;
            }
        }
    }
}
