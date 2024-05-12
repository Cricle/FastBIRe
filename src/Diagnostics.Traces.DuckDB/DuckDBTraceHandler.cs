using Diagnostics.Traces.Stores;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBTraceHandler<TIdentity> : IActivityTraceHandler, ILogRecordTraceHandler, IMetricTraceHandler, IBatchActivityTraceHandler, IBatchLogRecordTraceHandler, IBatchMetricTraceHandler, IDisposable
        where TIdentity : IEquatable<TIdentity>
    {
        public DuckDBTraceHandler(IUndefinedDatabaseSelector<DuckDBDatabaseCreatedResult> databaseSelector,
            IIdentityProvider<TIdentity, Activity>? activityIdentityProvider,
            IIdentityProvider<TIdentity, LogRecord>? logIdentityProvider,
            IIdentityProvider<TIdentity, Metric>? metricIdentityProvider = null)
        {
            DatabaseSelector = databaseSelector;
            ActivityIdentityProvider = activityIdentityProvider;
            LogIdentityProvider = logIdentityProvider;
            MetricIdentityProvider = metricIdentityProvider;
        }

        public IUndefinedDatabaseSelector<DuckDBDatabaseCreatedResult> DatabaseSelector { get; }

        public IIdentityProvider<TIdentity, Activity>? ActivityIdentityProvider { get; }

        public IIdentityProvider<TIdentity, LogRecord>? LogIdentityProvider { get; }

        public IIdentityProvider<TIdentity, Metric>? MetricIdentityProvider { get; }

        public void Dispose()
        {
        }

        public void Handle(Activity input)
        {
            throw new NotImplementedException();
        }

        public void Handle(LogRecord input)
        {
            throw new NotImplementedException();
        }

        public void Handle(Metric input)
        {
            throw new NotImplementedException();
        }

        public void Handle(in Batch<Activity> inputs)
        {
            throw new NotImplementedException();
        }

        public void Handle(in Batch<LogRecord> inputs)
        {
            throw new NotImplementedException();
        }

        public void Handle(in Batch<Metric> inputs)
        {
            throw new NotImplementedException();
        }


        public Task HandleAsync(Activity input, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(input);
            return Task.CompletedTask;
        }

        public Task HandleAsync(LogRecord input, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(input);
            return Task.CompletedTask;
        }

        public Task HandleAsync(Metric input, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(input);
            return Task.CompletedTask;
        }

        public Task HandleAsync(Batch<Activity> inputs, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(inputs);
            return Task.CompletedTask;
        }

        public Task HandleAsync(Batch<LogRecord> inputs, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(inputs);
            return Task.CompletedTask;
        }

        public Task HandleAsync(Batch<Metric> inputs, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Handle(inputs);
            return Task.CompletedTask;
        }
    }
}
