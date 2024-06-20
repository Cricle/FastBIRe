using System.Diagnostics;

namespace Diagnostics.Traces.Status
{
    public class ActivityStatus : IDisposable
    {
        private long startActivityCount;
        private long totalActivityCount;
        private long errorActivityCount;
        private int disposedCount;

        private readonly ActivityListener listener;

        public long StartActivityCount => Interlocked.Read(ref startActivityCount);

        public long TotalActivityCount => Interlocked.Read(ref totalActivityCount);

        public long ErrorActivityCount => Interlocked.Read(ref errorActivityCount);

        public ActivityStatus(ActivitySource source)
            : this(s => s == source)
        {

        }
        public ActivityStatus(string sourceName)
            : this(s => s.Name == sourceName)
        {

        }
        public ActivityStatus(Func<ActivitySource, bool> sourceFun)
        {
            if (sourceFun is null)
            {
                throw new ArgumentNullException(nameof(sourceFun));
            }

            listener = new ActivityListener
            {
                ShouldListenTo = sourceFun,
                ActivityStarted = ActivityStarted,
                ActivityStopped = ActivityStoped,
                Sample = static (ref ActivityCreationOptions<ActivityContext> x) => ActivitySamplingResult.AllData,
                SampleUsingParentId = static (ref ActivityCreationOptions<string> x) => ActivitySamplingResult.AllData
            };

            ActivitySource.AddActivityListener(listener);
        }

        private void ActivityStarted(Activity activity)
        {
            Interlocked.Increment(ref startActivityCount);
            Interlocked.Increment(ref totalActivityCount);

            OnActivityStarted(activity);
        }

        protected virtual void OnActivityStarted(Activity activity)
        {

        }

        private void ActivityStoped(Activity activity)
        {
            Interlocked.Decrement(ref startActivityCount);

            if (activity.Status == ActivityStatusCode.Error)
            {
                Interlocked.Decrement(ref errorActivityCount);
            }

            OnActivityStoped(activity);
        }

        protected virtual void OnActivityStoped(Activity activity)
        {

        }

        public void Dispose()
        {
            if (Interlocked.Increment(ref disposedCount) == 1)
            {
                listener.Dispose();
                OnDisposed();
            }
        }

        protected virtual void OnDisposed()
        {

        }
    }
}
