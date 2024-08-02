using System.Collections.Concurrent;
using System.Diagnostics;

namespace Diagnostics.Traces.Status
{
    public class SamplingActivityStatus : ActivityStatus, IDisposable
    {
        private ConcurrentDictionary<Activity, Activity> currentBuffer = null!;
        private TimerHandler? loopSwitchHandler;
        private bool willSwitch;

        public TimeSpan SwitchTime { get; }

        public int CurrentBufferSize => currentBuffer.Count;

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
            currentBuffer = new ConcurrentDictionary<Activity, Activity>();
            if (SwitchTime.Ticks != 0)
            {
                loopSwitchHandler = new TimerHandler(SwitchTime, Switch);
                willSwitch = true;
            }
        }

        public IList<Activity> UnsafeGetList()
        {
            if (willSwitch)
            {
                return Volatile.Read(ref currentBuffer).Values.ToList();
            }
            return currentBuffer.Values.ToList();

        }

        public void CopyBuffer(ICollection<Activity> activities)
        {
            var map = willSwitch ? Volatile.Read(ref currentBuffer) : currentBuffer;//TODO:
            foreach (var item in currentBuffer)
            {
                activities.Add(item.Value);
            }
        }

        public List<Activity> CopyBuffer()
        {
            return Volatile.Read(ref currentBuffer).Values.ToList();
        }

        protected override void OnActivityStarted(Activity activity)
        {
            if (willSwitch)
            {
                Volatile.Read(ref currentBuffer)[activity] = activity;
            }
            else
            {
                currentBuffer[activity] = activity;
            }
        }

        protected override void OnActivityStoped(Activity activity)
        {
            if (willSwitch)
            {
                Volatile.Read(ref currentBuffer).TryRemove(activity, out _);
            }
            else
            {
                currentBuffer.TryRemove(activity, out _);
            }
        }

        private void Switch()
        {
            Volatile.Write(ref currentBuffer, new ConcurrentDictionary<Activity, Activity>());
        }

        protected override void OnDisposed()
        {
            loopSwitchHandler?.Dispose();
        }
    }
}
