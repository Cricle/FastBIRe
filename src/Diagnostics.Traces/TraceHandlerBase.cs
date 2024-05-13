using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using System.Diagnostics;

namespace Diagnostics.Traces
{
    public abstract class TraceHandlerBase<TIdentity> : IActivityTraceHandler, ILogRecordTraceHandler, IMetricTraceHandler, IBatchActivityTraceHandler, IBatchLogRecordTraceHandler, IBatchMetricTraceHandler, IDisposable
        where TIdentity : IEquatable<TIdentity>
    {
        public virtual void Dispose()
        {
        }

        public abstract void Handle(Activity input);
        public abstract void Handle(LogRecord input);
        public abstract void Handle(Metric input);
        public abstract void Handle(in Batch<Activity> inputs);
        public abstract void Handle(in Batch<LogRecord> inputs);
        public abstract void Handle(in Batch<Metric> inputs);

        public virtual Task HandleAsync(Activity input, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(input);
            return Task.CompletedTask;
        }

        public virtual Task HandleAsync(LogRecord input, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(input);
            return Task.CompletedTask;
        }

        public virtual Task HandleAsync(Metric input, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(input);
            return Task.CompletedTask;
        }

        public virtual Task HandleAsync(Batch<Activity> inputs, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(inputs);
            return Task.CompletedTask;
        }

        public virtual Task HandleAsync(Batch<LogRecord> inputs, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(inputs);
            return Task.CompletedTask;
        }

        public virtual Task HandleAsync(Batch<Metric> inputs, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(inputs);
            return Task.CompletedTask;
        }
    }

}
