using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Diagnostics.Helpers
{
    public static class SampleHelper
    {
        private static readonly Dictionary<string, string> oneSecondInterval = new Dictionary<string, string>(1)
        {
            ["EventCounterIntervalSec"] = "1"
        };

        private static ICounterResult GetCounterResult(int processId, int eventFlushintervalSec)
        {
            var map = eventFlushintervalSec == 1 ? oneSecondInterval : new Dictionary<string, string>(1)
            {
                ["EventCounterIntervalSec"] = eventFlushintervalSec.ToString()
            };
            return CounterHelper.CreateCounter(processId, new[] { new EventPipeProvider(WellKnowsEventProvider.Runtime, EventLevel.Informational, (long)ClrTraceEventParser.Keywords.None, map) });
        }
        public static Task<RuntimeEventCounter> OnceAsync(CancellationToken token = default)
        {
            return OnceAsync(PlatformHelper.CurrentProcessId, token);
        }
        public static async Task<RuntimeEventCounter> OnceAsync(int processId, CancellationToken token = default)
        {
            using (var sample = GetIntervalRuntimeSample(processId))
            {
                await sample.OnceAsync(token);
                return sample.Counter;
            }
        }
        public static ISampleResult GetIntervalRuntimeSample(int processId, TimeSpan? interval = null, int eventFlushintervalSec = 1)
        {
            var res = GetCounterResult(processId, eventFlushintervalSec);
            return new SampleResult(res, new IntervalRuntimeEventCounter(interval ?? TimeSpan.FromSeconds(1)));
        }
        public static ISampleResult GetRuntimeSample(int processId, int eventFlushintervalSec = 1)
        {
            var res = GetCounterResult(processId, eventFlushintervalSec);
            return new SampleResult(res, new RuntimeEventCounter());
        }
        class SampleResult : ISampleResult
        {
            private long isStop = 1;
            private readonly Task task;
            private readonly CancellationTokenSource tokenSource;

            public RuntimeEventCounter Counter { get; }

            public ICounterResult CounterResult { get; }

            public bool IsStop => Interlocked.Read(ref isStop) != 0;

            public Task Task => task;

            public SampleResult(ICounterResult counterResult, RuntimeEventCounter counter)
            {
                CounterResult = counterResult;
                Counter = counter;
                tokenSource = new CancellationTokenSource();
                task = Task.Factory.StartNew(() => CounterResult.StartSessionAsync(tokenSource.Token)).Unwrap();
                Resume();
            }

            private void OnPayloadReceived(object? sender, ICounterPayload e)
            {
                Counter.Update(e);
            }

            public void Dispose()
            {
                tokenSource.Cancel();
                if (Counter is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            public async Task OnceAsync(Action<RuntimeEventCounter> action, CancellationToken token)
            {
                await OnceAsync(token);
                action(Counter);
            }

            public void Pause()
            {
                if (Interlocked.CompareExchange(ref isStop,1,0)==0)
                {
                    CounterResult.PayloadReceived -= OnPayloadReceived;
                }
            }

            public void Resume()
            {
                if (Interlocked.CompareExchange(ref isStop, 0, 1) == 1)
                {
                    CounterResult.PayloadReceived += OnPayloadReceived;
                }
            }

            public async Task<RuntimeEventCounter> OnceAsync(CancellationToken token)
            {
                using (var ts = new CancellationTokenSource())
                {
                    token.Register(() => ts.Cancel());
                    var startTask = Task.Factory.StartNew(() => CounterResult.StartSessionAsync(ts.Token));
                    var onceTask = Counter.OnceAsync(token);
                    await onceTask;
                    ts.Cancel();
                    await startTask;
                    return Counter;
                }
            }
        }
    }
}
