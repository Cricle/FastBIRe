﻿using Diagnostics.Traces.Models;
using Diagnostics.Traces.Serialization;

namespace Diagnostics.Traces.Mini
{
    public class MiniTraceReader : ITraceReader, IDisposable
    {
        public MiniTraceReader(Stream stream)
            :this(new StreamMiniReadSerializer(stream))
        {

        }
        public MiniTraceReader(IMiniReadSerializer miniReadSerializer)
        {
            MiniReadSerializer = miniReadSerializer;
        }

        public IMiniReadSerializer MiniReadSerializer { get; }

        public void Dispose()
        {
            if (MiniReadSerializer is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        public IEnumerable<BytesStoreValue> ReadBytesStoreValues()
        {
            var helper = new MiniReadTraceHelper(MiniReadSerializer);
            var head = helper.ReadHead();
            while (true)
            {
                var result = helper.ReadBytesStoreValue();
                if (result==null)
                {
                    yield break;
                }
                yield return result.Value;
            }
        }
        public IEnumerable<AcvtityEntity> ReadActivities(IEnumerable<string>? traceIds = null)
        {
            var helper = new MiniReadTraceHelper(MiniReadSerializer);
            var head = helper.ReadHead();
            while (true)
            {
                var result = helper.ReadActivity();
                if (result.ResultType != MiniReadResultTypes.Succeed)
                {
                    yield break;
                }
                yield return result.Result!;
            }
        }

        public IEnumerable<ExceptionEntity> ReadExceptions(IEnumerable<string>? traceIds = null)
        {

            var helper = new MiniReadTraceHelper(MiniReadSerializer);
            var head = helper.ReadHead();
            while (true)
            {
                var result = helper.ReadException();
                if (result.ResultType != MiniReadResultTypes.Succeed)
                {
                    yield break;
                }
                yield return result.Result!;
            }
        }

        public IEnumerable<LogEntity> ReadLogs(IEnumerable<string>? traceIds = null)
        {
            var helper = new MiniReadTraceHelper(MiniReadSerializer);
            var head = helper.ReadHead();
            while (true)
            {
                var result = helper.ReadLog();
                if (result.ResultType != MiniReadResultTypes.Succeed)
                {
                    yield break;
                }
                yield return result.Result!;
            }
        }

        public IEnumerable<MetricEntity> ReadMetrics()
        {
            throw new NotImplementedException();
        }
    }
}
