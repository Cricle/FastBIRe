using System;
using System.Threading;

namespace Diagnostics.Helpers
{
    public class IntervalRuntimeEventCounter : RuntimeEventCounter, IDisposable
    {
        private int isChanged;
        private readonly Timer timer;

        public IntervalRuntimeEventCounter(TimeSpan interval)
        {
            timer = new Timer(OnTimerRaise, this, TimeSpan.Zero, interval);
        }
        protected override void OnUpdated(ICounterPayload payload)
        {
            Interlocked.CompareExchange(ref isChanged, 1, 0);
        }
        private void OnTimerRaise(object? state)
        {
            if (Interlocked.CompareExchange(ref isChanged, 0, 1) == 1)
            {
                RaiseChanged();
            }
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
