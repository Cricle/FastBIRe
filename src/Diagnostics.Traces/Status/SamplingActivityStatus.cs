using System.Diagnostics;

namespace Diagnostics.Traces.Status
{
    public class SamplingActivityStatus : ActivityStatus, IDisposable
    {
        private LinkedList<Activity> currentBuffer = null!;
        private TimerHandler loopSwitchHandler = null!;

        private readonly object locker = new object();

        public TimeSpan SwitchTime { get; }

        public int CurrentBufferSize
        {
            get
            {
                lock (locker)
                {
                    return currentBuffer.Count;
                }
            }
        }

        public SamplingActivityStatus(ActivitySource source, TimeSpan switchTime)
            : base(source)
        {
            SwitchTime = switchTime;
            Init();
        }

        public SamplingActivityStatus(string sourceName, TimeSpan switchTime)
            : base(sourceName)
        {
            SwitchTime = switchTime;
            Init();
        }

        public SamplingActivityStatus(Func<ActivitySource, bool> sourceFun, TimeSpan switchTime)
            : base(sourceFun)
        {
            SwitchTime = switchTime;
            Init();
        }

        private void Init()
        {
            currentBuffer = new LinkedList<Activity>();
            loopSwitchHandler = new TimerHandler(SwitchTime, Switch);
        }

        public LinkedList<Activity> UnsafeGetList()
        {
            return currentBuffer;
        }

        public void CopyBuffer(ICollection<Activity> activities)
        {
            lock (locker)
            {
                foreach (var item in currentBuffer)
                {
                    activities.Add(item);
                }
            }
        }

        public List<Activity> CopyBuffer()
        {
            lock (locker)
            {
                return currentBuffer.ToList();
            }
        }

        protected override void OnActivityStarted(Activity activity)
        {
            lock (locker)
            {
                currentBuffer.AddLast(activity);
            }
        }

        protected override void OnActivityStoped(Activity activity)
        {
            lock (locker)
            {
                currentBuffer.Remove(activity);
            }
        }

        private void Switch()
        {
            lock (locker)
            {
                currentBuffer = new LinkedList<Activity>();
            }
        }

        protected override void OnDisposed()
        {
            loopSwitchHandler.Dispose();
        }
    }
}
