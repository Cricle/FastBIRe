namespace Diagnostics.Traces.Zips
{
    public class ZipTraceManager<TIdentity> : LruCache<TIdentity, ZipTraceEntry>
        where TIdentity : IEquatable<TIdentity>
    {
        private readonly CancellationTokenSource tokenSource;

        public ZipTraceManager(TimeSpan removeWhenNoVisitTime)
        {
            if (removeWhenNoVisitTime.TotalMilliseconds<=0)
            {
                throw new ArgumentException("The removeWhenNoVisitTime must more than zero");
             }
            RemoveWhenNoVisitTime = removeWhenNoVisitTime;
            tokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(LoopDispose, this, tokenSource.Token);
        }
        ~ZipTraceManager()
        {
            Dispose();
        }
        public TimeSpan RemoveWhenNoVisitTime { get; }

        protected override void DisposeValue(ZipTraceEntry value)
        {
            value.Dispose();
        }

        private void LoopDispose(object? state)
        {
            var mgr = (ZipTraceManager<TIdentity>)state!;
            var ts = mgr.tokenSource;
            var removeWhenNoVisitTime = mgr.RemoveWhenNoVisitTime;

            while (!ts.IsCancellationRequested)
            {
                try
                {
                    lock (mgr.locker)
                    {
                        var removeKeys = new List<KeyValuePair<TIdentity, Node<TIdentity, ZipTraceEntry>>>(0);
                        foreach (var item in mgr.data)
                        {
                            if (item.Value.Value.LastVisitIsLarged(removeWhenNoVisitTime))
                            {
                                removeKeys.Add(item);
                            }
                        }
                        if (removeKeys.Count != 0)
                        {
                            foreach (var item in removeKeys)
                            {
                                mgr.data.Remove(item.Key);
                                RemoveNodeFromList(item.Value);
                                DisposeValue(item.Value.Value);
                            }
                        }
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {

                }
            }

            ts.Dispose();
        }
        protected override void OnDisposed()
        {
            tokenSource.Cancel();
        }
    }
}
