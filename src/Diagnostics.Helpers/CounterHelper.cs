using Microsoft.Diagnostics.Monitoring.EventPipe;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public static class CounterHelper
    {
        public static ICounterResult CreateCounter(int processId, IEnumerable<EventPipeProvider> providers,CounterConfiguration? configuration=null, bool requestRundown = true, int bufferSizeInMB = 256)
        {
            configuration ??= new CounterConfiguration(CounterFilter.AllCounters(1));
            var client = new DiagnosticsClient(processId);
            return new CounterResult(processId, providers, configuration, requestRundown, bufferSizeInMB,client);
        }
    }
    public readonly record struct ExceptionReceivedPlayload
    {
        public ExceptionReceivedPlayload(TraceEvent traceEvent, Exception exception)
        {
            TraceEvent = traceEvent;
            Exception = exception;
        }

        public TraceEvent TraceEvent { get; }

        public Exception Exception { get; }
    }
    public interface ICounterResult
    {
        event EventHandler<ICounterPayload>? PayloadReceived;

        event EventHandler<ExceptionReceivedPlayload>? ExceptionReceived;

        Task StartSessionAsync( CancellationToken token);
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
            var taskSource = new TaskCompletionSource<bool>();
            token.Register(() => taskSource.SetResult(true));
            using (var session = Client.StartEventPipeSession(Providers, RequestRundown, BufferSizeInMB))
            using (var source = new EventPipeEventSource(session.EventStream))
            {
                Client.ResumeRuntime();
                source.Dynamic.All += (traceEvent) =>
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
                        ExceptionReceived?.Invoke(this, new ExceptionReceivedPlayload(traceEvent, ex));
                    }
                };
                source.Process();
                await taskSource.Task;
            }
        }
    }
}
