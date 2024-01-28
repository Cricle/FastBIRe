using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public static class SampleHelper
    {
        private static ICounterResult GetCounterResult(int processId,int eventFlushintervalSec)
        {
            return CounterHelper.CreateCounter(processId, new[] { new EventPipeProvider(WellKnowsEventProvider.Runtime, EventLevel.Informational,(long)ClrTraceEventParser.Keywords.None,
                new Dictionary<string, string>
                {
                   ["EventCounterIntervalSec"]=eventFlushintervalSec.ToString()
                })
            });
        }

        public static ISampleResult GetIntervalRuntimeSample(int processId, TimeSpan interval, int eventFlushintervalSec=1)
        {
            var res = GetCounterResult(processId,eventFlushintervalSec);
            return new SampleResult(res, new IntervalRuntimeEventCounter(interval));
        }
        public static ISampleResult GetRuntimeSample(int processId, int eventFlushintervalSec = 1)
        {
            var res = GetCounterResult(processId, eventFlushintervalSec);
            return new SampleResult(res, new RuntimeEventCounter());
        }
        class SampleResult : ISampleResult
        {
            public RuntimeEventCounter Counter { get; }

            public ICounterResult CounterResult { get; }

            public SampleResult(ICounterResult counterResult, RuntimeEventCounter counter)
            {
                CounterResult = counterResult;
                Counter = counter;
            }

            public async Task StartAsync(CancellationToken token)
            {
                CounterResult.PayloadReceived += OnPayloadReceived;
                try
                {
                    await CounterResult.StartSessionAsync(token);
                }
                finally
                {
                    CounterResult.PayloadReceived -= OnPayloadReceived;
                }
            }

            private void OnPayloadReceived(object? sender, ICounterPayload e)
            {
                Counter.Update(e);
            }

            public void Dispose()
            {
                if (Counter is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

    }
}
