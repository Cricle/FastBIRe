using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Diagnostics.Traces.Zips
{
    public delegate Task ZipTraceHandler<TIdentity, TInput>(TIdentity identity, TInput input, ZipTraceEntry entry, CancellationToken token);
    public delegate Task ZipTraceStreamHandler<TIdentity, TInput>(TIdentity identity, TInput input, Stream stream, CancellationToken token);

    public delegate void ZipTraceHandlerSync<TIdentity, TInput>(TIdentity identity, TInput input, ZipTraceEntry entry);
    public delegate void ZipTraceStreamHandlerSync<TIdentity, TInput>(TIdentity identity, TInput input, Stream stream);

    public class ZipTraceHandler<TIdentity> : IActivityTraceHandler, ILogRecordTraceHandler, IMetricTraceHandler, IDisposable
        where TIdentity : IEquatable<TIdentity>
    {
        public const string ActivityEntryName = "activity";
        public const string LogEntryName = "log";
        public const string MetricEntryName = "metric";

        private static readonly byte[] newLineBuffer=Encoding.UTF8.GetBytes(Environment.NewLine);

        public ZipTraceHandler(ZipTraceManager<TIdentity> zipTraceManager,
            IPhysicalPathProvider<TIdentity> physicalPathProvider,
            IIdentityProvider<TIdentity, Activity>? activityIdentityProvider,
            IIdentityProvider<TIdentity, LogRecord>? logIdentityProvider,
            IIdentityProvider<TIdentity, Metric>? metricIdentityProvider = null)
        {
            ZipTraceManager = zipTraceManager;
            PhysicalPathProvider = physicalPathProvider;
            ActivityIdentityProvider = activityIdentityProvider;
            LogIdentityProvider = logIdentityProvider;
            MetricIdentityProvider = metricIdentityProvider;
        }

        public ZipTraceManager<TIdentity> ZipTraceManager { get; }

        public IPhysicalPathProvider<TIdentity> PhysicalPathProvider { get; }

        public IIdentityProvider<TIdentity,Activity>? ActivityIdentityProvider { get; }

        public IIdentityProvider<TIdentity,LogRecord>? LogIdentityProvider { get; }

        public IIdentityProvider<TIdentity,Metric>? MetricIdentityProvider { get; }

        private bool TryGetIdentity<TInput>(IIdentityProvider<TIdentity, TInput>? provider, TInput input,out TIdentity? identity)
        {
            identity = default;

            if (provider == null)
            {
                return false;
            }

            var res= provider.GetIdentity(input);
            if (res.Succeed)
            {
                identity = res.Identity;
                return true;
            }
            return false;
        }
        private bool TryGetEntry<TInput>(IIdentityProvider<TIdentity, TInput>? provider, TInput input,out TIdentity? identity, out ZipTraceEntry? entry)
        {
            if (!TryGetIdentity(provider, input, out identity) || identity == null)
            {
                entry = null;
                return false;
            }

            entry= ZipTraceManager.GetOrAdd(identity, k => new ZipTraceEntry(PhysicalPathProvider.GetPath(k)));
            return true;
        }
        protected void HandleCore<TInput>(IIdentityProvider<TIdentity, TInput>? provider, TInput input, ZipTraceHandlerSync<TIdentity, TInput> handle)
        {
            if (!TryGetEntry(provider,input,out var identity,out var entry)|| entry == null||identity==null)
            {
                return;
            }
            entry.Slim.Wait();
            try
            {
                handle(identity, input, entry);
            }
            finally
            {
                entry.Slim.Release();
            }
        }
        protected async Task HandleCoreAsync<TInput>(IIdentityProvider<TIdentity, TInput>? provider, TInput input, ZipTraceHandler<TIdentity, TInput> handle, CancellationToken token)
        {
            if (!TryGetEntry(provider, input, out var identity, out var entry) || entry == null || identity == null)
            {
                return;
            }

            await entry.Slim.WaitAsync(token);
            try
            {
                await handle(identity, input, entry, token);
            }
            finally
            {
                entry.Slim.Release();
            }
        }
        protected void HandleWithStreamEnd<TInput>(IIdentityProvider<TIdentity, TInput>? provider, TInput input, ZipTraceStreamHandlerSync<TIdentity, TInput> handle, string entryName)
        {
            HandleCore(provider, input, (identity, activity, entity) =>
            {
                using (var stream = entity.GetOrCreateOpenStream(entryName))
                {
                    stream.Seek(0, SeekOrigin.End);

                    handle(identity, activity, stream);
                }
            });
        }
        protected Task HandleWithStreamEndAsync<TInput>(IIdentityProvider<TIdentity, TInput>? provider, TInput input, ZipTraceStreamHandler<TIdentity, TInput> handle,string entryName, CancellationToken token)
        {
            return HandleCoreAsync(provider, input, async (identity, activity, entity,token) =>
            {
                using (var stream = entity.GetOrCreateOpenStream(entryName))
                {
                    stream.Seek(0, SeekOrigin.End);

                    await handle(identity, activity, stream, token).ConfigureAwait(false);
                }
            }, token);
        }

        public Task HandleAsync(Activity input, CancellationToken token)
        {
            return HandleWithStreamEndAsync(ActivityIdentityProvider, input, async (identity, activity, stream, token) =>
            {
                WriteActivityJson(stream, activity);
                await stream.WriteAsync(newLineBuffer,0, newLineBuffer.Length,token);
            },input.TraceId.ToString(), token);
        }
        private string LogToString(LogRecord log)
        {
            return $"{log.Timestamp.ToLocalTime():yyyy-MM-dd HH:mm:ss.ffff},{log.LogLevel},{log.CategoryName},{log.TraceId},{log.SpanId},{log.Attributes}";
        }
        public Task HandleAsync(LogRecord input, CancellationToken token)
        {
            return HandleWithStreamEndAsync(LogIdentityProvider, input,async (identity, log, stream, token) =>
            {
                var str = LogToString(log);
                await stream.WriteStringAsync(str, token);
                
                await stream.WriteAsync(newLineBuffer, 0, newLineBuffer.Length, token);
            },LogEntryName, token);
        }

        public Task HandleAsync(Metric input, CancellationToken token)
        {
            return HandleWithStreamEndAsync(MetricIdentityProvider, input, (identity, metric, stream, token) =>
            {
                using (var writer=new StreamWriter(stream))
                {
                    MetricExportHelper.ExportMetricString(writer, metric);
                }
                return Task.CompletedTask;
            },MetricEntryName, token);
        }

        public void Dispose()
        {
            ZipTraceManager.Dispose();
        }
        private void WriteActivityJson(Stream stream,Activity activity)
        {
            using (var utf8Writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                SkipValidation =
#if DEBUG
                       false
#else
                        true
#endif
            }))
            {
                ActivityJsonConverter.Write(utf8Writer, activity);
            }
        }
        public void Handle(Activity input)
        {
            HandleWithStreamEnd(ActivityIdentityProvider, input, (identity, activity, stream) =>
            {
                WriteActivityJson(stream, activity);
                stream.Write(newLineBuffer, 0, newLineBuffer.Length);
            }, input.TraceId.ToString());

        }

        public void Handle(LogRecord input)
        {
            HandleWithStreamEnd(LogIdentityProvider, input, (identity, log, stream) =>
            {
                var str = LogToString(log);
                stream.WriteString(str);

                stream.Write(newLineBuffer, 0, newLineBuffer.Length);
            }, LogEntryName);
        }

        public void Handle(Metric input)
        {
            HandleWithStreamEnd(MetricIdentityProvider, input, (identity, metric, stream) =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    MetricExportHelper.ExportMetricString(writer, metric);
                }
            }, MetricEntryName);
        }
    }
}
