namespace Diagnostics.Traces.Models
{
    public class TraceData
    {
        public TraceData(Dictionary<TraceKey, List<LogEntity>> logs, Dictionary<TraceKey, List<ActivityEntity>> acvtities, Dictionary<TraceKey, List<ExceptionEntity>> exceptions)
        {
            Logs = logs;
            Acvtities = acvtities;
            Exceptions = exceptions;
        }

        public static TraceData Create(IEnumerable<LogEntity> logs, IEnumerable<ActivityEntity> acvtities, IEnumerable<ExceptionEntity> exceptions)
        {
            var tlogs = logs.GroupBy(static x => x.GetTraceKey()).ToDictionary(static x => x.Key, static x => x.ToList());
            var tactivities = acvtities.GroupBy(static x => x.GetParentTraceKey()).ToDictionary(static x => x.Key, static x => x.ToList());
            var texceptions = exceptions.GroupBy(static x => x.GetTraceKey()).ToDictionary(static x => x.Key, static x => x.ToList());
            return new TraceData(tlogs, tactivities, texceptions);
        }

        public Dictionary<TraceKey, List<LogEntity>> Logs { get; }

        public Dictionary<TraceKey, List<ActivityEntity>> Acvtities { get; }

        public Dictionary<TraceKey, List<ExceptionEntity>> Exceptions { get; }

        public List<TraceTree> BuildTrees()
        {
            var trees = new List<TraceTree>();
            var allRootActivities = Acvtities.Values.SelectMany(x => x.Where(y => y.IsRootSpan()));
            foreach (var activity in allRootActivities)
            {
                var tree = new TraceTree(activity);
                BuildTree(tree);
                trees.Add(tree);
            }
            return trees;
        }

        private void BuildTree(TraceTree rootTree)
        {
            var treaceKey = rootTree.Activity.GetTraceKey();
            if (Logs.TryGetValue(treaceKey, out var logs))
            {
                rootTree.Logs.AddRange(logs);
            }
            if (Exceptions.TryGetValue(treaceKey, out var exceptions))
            {
                rootTree.Exceptions.AddRange(exceptions);
            }
            if (Acvtities.TryGetValue(rootTree.Activity.GetTraceKey(), out var entities))
            {
                foreach (var entry in entities)
                {
                    var tree = new TraceTree(entry);
                    BuildTree(tree);
                    rootTree.Nexts.Add(tree);
                }
            }
        }
    }

    public record class TraceTree
    {
        public TraceTree(ActivityEntity rootActivity)
        {
            Activity = rootActivity;
            Nexts = new List<TraceTree>(0);
            Exceptions = new List<ExceptionEntity>(0);
            Logs = new List<LogEntity>(0);
        }

        public ActivityEntity Activity { get; }

        public List<TraceTree> Nexts { get; }

        public List<ExceptionEntity> Exceptions { get; }

        public List<LogEntity> Logs { get; }
    }
}
