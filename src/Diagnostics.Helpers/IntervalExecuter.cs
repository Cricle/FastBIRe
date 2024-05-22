using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public class IntervalExecuter : IIntervalExecuter
    {
        private bool isRunning;
        private readonly Task task;
        private readonly CancellationTokenSource tokenSource;


        public IntervalExecuter(TimeSpan interval, Func<Task> executer)
        {
            if (interval.TotalMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException($"The interval must more than 1s");
            }
            Interval = interval;
            Executer = executer;
            tokenSource = new CancellationTokenSource();
            task = Task.Factory.StartNew(LoopAsync, this, TaskCreationOptions.LongRunning);
            isRunning = true;
        }

        public TimeSpan Interval { get; }

        public Func<Task> Executer { get; }

        public Task IntervalTask => task;

        public bool IsRunning => isRunning;

        public event EventHandler<Exception>? ExceptionRaised;

        private async Task LoopAsync(object? state)
        {
            var executer = (IntervalExecuter)state!;
            long startTime = 0;
            long subTime = 0;
            var handler = executer.Executer;
            var intervalTicks = executer.Interval.Ticks;
            while (!executer.tokenSource.IsCancellationRequested)
            {
                try
                {
                    startTime = Stopwatch.GetTimestamp();
                    await handler();
                }
                catch (Exception ex)
                {
                    executer.ExceptionRaised?.Invoke(executer, ex);
                }
                finally
                {
                    subTime = intervalTicks-( Stopwatch.GetTimestamp() - startTime);
                    if (subTime < executer.Interval.Ticks)
                    {
                        await Task.Delay((int)(subTime / TimeSpan.TicksPerMillisecond));
                    }
                }
            }
        }

        public void Dispose()
        {
            tokenSource.Cancel();
            isRunning = false;
        }
    }
}
