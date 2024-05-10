using LiteDB;
using System.Diagnostics;

namespace Diagnostics.Traces.LiteDb
{
    internal static class ActivityToLiteHelper
    {
        public const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static void Write(BsonDocument doc, Activity value)
        {
            doc.Add("Id", value.Id);
            doc.Add("Status", value.Status.ToString());
            doc.Add("StatusDescription", value.StatusDescription);
            doc.Add("HasRemoteParent", value.HasRemoteParent);
            doc.Add("Kind", value.Kind.ToString());
            doc.Add("OperationName", value.OperationName);
            doc.Add("DisplayName", value.DisplayName);

            var source = new BsonDocument
            {
                {"Name", value.Source.Name},
                {"Version", value.Source.Version}
            };
            doc.Add("Source", source);

            doc.Add("Duration", value.Duration.ToString());
            doc.Add("StartTimeUtc", value.StartTimeUtc.ToLocalTime().ToString(TimeFormat));
            doc.Add("ParentId", value.ParentId);
            doc.Add("RootId", value.RootId);

            var tags = new BsonArray();
            foreach (var tag in value.Tags)
            {
                var tagDoc = new BsonDocument
                {
                    {"Key", tag.Key},
                    {"Value", tag.Value}
                };
                tags.Add(tagDoc);
            }
            doc.Add("Tags", tags);

            var events = new BsonArray();
            foreach (var activityEvent in value.Events)
            {
                var eventTags = new BsonArray();
                foreach (var tag in activityEvent.Tags)
                {
                    var tagDoc = new BsonDocument
                    {
                        {"Key", tag.Key},
                        {"Value", tag.Value?.ToString()}
                    };
                    eventTags.Add(tagDoc);
                }

                var eventDoc = new BsonDocument
                {
                    {"Name", activityEvent.Name},
                    {"Timestamp", activityEvent.Timestamp.ToLocalTime().ToString(TimeFormat)},
                    {"Tags", eventTags}
                };
                events.Add(eventDoc);
            }
            doc.Add("Events", events);

            var links = new BsonArray();
            foreach (var link in value.Links)
            {
                var linkContext = new BsonDocument
                {
                    {"TraceId", link.Context.TraceId.ToString()},
                    {"TraceState", link.Context.TraceState},
                    {"TraceFlags", link.Context.TraceFlags.ToString()},
                    {"IsRemote", link.Context.IsRemote},
                    {"SpanId", link.Context.SpanId.ToString()}
                };

                var linkTags = new BsonDocument();
                if (link.Tags != null)
                {
                    foreach (var item in link.Tags)
                    {
                        linkTags.Add(item.Key, item.Value?.ToString());
                    }
                }

                var linkDoc = new BsonDocument
                {
                    {"Context", linkContext},
                    {"Tags", linkTags}
                };
                links.Add(linkDoc);
            }
            doc.Add("Links", links);

            var baggage = new BsonArray();
            foreach (var baggageItem in value.Baggage)
            {
                var baggageDoc = new BsonDocument
                {
                    {"Key", baggageItem.Key},
                    {"Value", baggageItem.Value}
                };
                baggage.Add(baggageDoc);
            }
            doc.Add("Baggage", baggage);

            var context = new BsonDocument
            {
                {"TraceId", value.Context.TraceId.ToString()},
                {"SpanId", value.Context.SpanId.ToString()},
                {"TraceFlags", value.Context.TraceFlags.ToString()},
                {"IsRemote", value.Context.IsRemote}
            };
            doc.Add("Context", context);

            doc.Add("TraceStateString", value.TraceStateString);
            doc.Add("SpanId", value.SpanId.ToString());
            doc.Add("TraceId", value.TraceId.ToString());
            doc.Add("Recorded", value.Recorded);
            doc.Add("IsAllDataRequested", value.IsAllDataRequested);
            doc.Add("ActivityTraceFlags", value.ActivityTraceFlags.ToString());
            doc.Add("ParentSpanId", value.ParentSpanId.ToString());
            doc.Add("IsStopped", value.IsStopped);
            doc.Add("IdFormat", value.IdFormat.ToString());
        }
    }
}
