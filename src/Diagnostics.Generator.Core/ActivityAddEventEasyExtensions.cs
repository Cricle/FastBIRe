using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Diagnostics.Generator.Core
{
    public static class ActivityAddEventEasyExtensions
    {
        public static Activity? StartActivity(this ActivitySource source, string name, ActivityKind kind = ActivityKind.Internal, ActivityContext parentContext = default, IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<ActivityLink>? links = null, DateTimeOffset startTime = default)
        {
            return source.StartActivity(name, kind, parentContext, tags, links, startTime);
        }
        public static void AddEvent(this Activity activity, string name, ActivityTagsCollection? tags = null, DateTimeOffset timestamp = default)
        {
            if (activity == null)
            {
                return;
            }

            activity.AddEvent(new ActivityEvent(name, timestamp, tags));
        }
        public static void AddEvent(this Activity activity, string name, Dictionary<string, object?> tags, DateTimeOffset timestamp = default)
        {
            if (activity == null)
            {
                return;
            }

            activity.AddEvent(name, (IEnumerable<KeyValuePair<string, object?>>)tags, timestamp);
        }
        public static void AddEvent(this Activity activity, string name, IEnumerable<KeyValuePair<string, object?>> tags, DateTimeOffset timestamp = default)
        {
            if (activity == null)
            {
                return;
            }

            activity.AddEvent(new ActivityEvent(name, timestamp, new ActivityTagsCollection(tags)));
        }
        public static void AddEvent(this Activity activity, string name, params (string, object?)[] tags)
        {
            activity.AddEvent(name, tags, default);
        }
        public static void AddEvent(this Activity activity, string name, IEnumerable<(string, object?)> tags, DateTimeOffset timestamp)
        {
            if (activity == null)
            {
                return;
            }
            var coll = new ActivityTagsCollection();
            foreach (var item in tags)
            {
                coll[item.Item1] = item.Item2;
            }
            activity.AddEvent(name, coll, timestamp);
        }
    }
}
