using Diagnostics.Generator.Core;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace Diagnostics.Traces
{
    public enum ExceptionCatchMode
    {
        Full = 0,
        OnlyHasActivity = 1
    }
    public class ExceptionMonitor : IDisposable
    {
        public ExceptionMonitor(IBatchOperatorHandler<TraceExceptionInfo> exceptionHandler, int bufferSize = 512, int swapDelayTimeMs = 5000)
        {
            ExceptionHandler = exceptionHandler;
            exceptionOperator = new BatchBufferOperator<TraceExceptionInfo>(exceptionHandler, bufferSize, swapDelayTimeMs);
            AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
        }

        private void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
        {
            var activity = Activity.Current;
            if (CatchMode == ExceptionCatchMode.OnlyHasActivity && activity == null)
            {
                return;
            }
            exceptionOperator.Add(new TraceExceptionInfo(e.Exception, activity?.TraceId, activity?.SpanId));
        }

        private readonly BatchBufferOperator<TraceExceptionInfo> exceptionOperator;

        public IBatchOperatorHandler<TraceExceptionInfo> ExceptionHandler { get; }

        public ExceptionCatchMode CatchMode { get; set; } = ExceptionCatchMode.Full;

        public void Dispose()
        {
            AppDomain.CurrentDomain.FirstChanceException -= OnFirstChanceException;
            exceptionOperator.Dispose();
        }
    }
}
