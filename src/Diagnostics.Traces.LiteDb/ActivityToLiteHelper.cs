using LiteDB;
using System.Diagnostics;

namespace Diagnostics.Traces.LiteDb
{
    internal static class ActivityToLiteHelper
    {
        public const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static void Write(BsonDocument doc, Activity value)
        {
            doc.Add("_id", value.Id);
            doc.Add("Status", (int)value.Status);
            if (!string.IsNullOrEmpty(value.StatusDescription))
            {
                doc.Add("StatusDescription", value.StatusDescription);
            }
            doc.Add("HasRemoteParent", value.HasRemoteParent);
            doc.Add("Kind", (int)value.Kind);
            doc.Add("OperationName", value.OperationName);
            doc.Add("DisplayName", value.DisplayName);

            doc.Add("Source.Name", value.Source.Name);
            if (!string.IsNullOrEmpty(value.Source.Version))
            {
                doc.Add("Source.Version", value.Source.Version);
            }

            doc.Add("Duration", value.Duration.TotalMilliseconds);
            doc.Add("StartTimeUtc", value.StartTimeUtc);
            if (!string.IsNullOrEmpty(value.ParentId))
            {
                doc.Add("ParentId", value.ParentId);
            }
            doc.Add("RootId", value.RootId);
            if (value.Tags.Any())
            {
                var tags = new BsonDocument();
                foreach (var tag in value.Tags)
                {
                    tags[tag.Key] = tag.Value;
                }
                doc.Add("Tags", tags);
                if (value.Events.Any())
                {
                    var events = new BsonArray();
                    foreach (var activityEvent in value.Events)
                    {
                        var eventTags = new BsonDocument();
                        foreach (var tag in activityEvent.Tags)
                        {
                            eventTags[tag.Key] = tag.Value?.ToString();
                        }

                        var eventDoc = new BsonDocument
                    {
                        {"Name", activityEvent.Name},
                        {"Timestamp", activityEvent.Timestamp.DateTime.ToLocalTime()},
                        {"Tags", eventTags}
                    };
                        events.Add(eventDoc);
                    }
                    doc.Add("Events", events);
                }
            }

            if (value.Links.Any())
            {
                var links = new BsonArray();
                foreach (var link in value.Links)
                {
                    var linkContext = new BsonDocument
                    {
                        {"TraceId", link.Context.TraceId.ToString()},
                        {"TraceState", link.Context.TraceState},
                        {"TraceFlags", (int)link.Context.TraceFlags},
                        {"IsRemote", link.Context.IsRemote},
                        {"SpanId", link.Context.SpanId.ToString()}
                    };

                    var linkTags = new BsonDocument();
                    if (link.Tags != null)
                    {
                        foreach (var item in link.Tags)
                        {
                            linkTags[item.Key] = item.Value?.ToString();
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
            }

            if (value.Baggage.Any())
            {
                var baggage = new BsonDocument();
                foreach (var baggageItem in value.Baggage)
                {
                    baggage[baggageItem.Key] = baggageItem.Value;
                }
                doc.Add("Baggage", baggage);
            }
            if (value.Context.TraceId != value.TraceId &&
                value.Context.SpanId != value.SpanId)
            {
                var context = new BsonDocument
                {
                    {"TraceId", value.Context.TraceId.ToString()},
                    {"SpanId", value.Context.SpanId.ToString()},
                    {"TraceFlags", (int)value.Context.TraceFlags},
                    {"IsRemote", value.Context.IsRemote}
                };
                doc.Add("Context", context);
            }
            
            if (!string.IsNullOrEmpty(value.TraceStateString))
            {
                doc.Add("TraceStateString", value.TraceStateString);
            }

            doc.Add("SpanId", value.SpanId.ToString());
            doc.Add("TraceId", value.TraceId.ToString());
            doc.Add("Recorded", value.Recorded);
            doc.Add("ActivityTraceFlags", (int)value.ActivityTraceFlags);
            if (!value.ParentSpanId.Equals(default))
            {
                doc.Add("ParentSpanId", value.ParentSpanId.ToString());
            }
        }
    }
}
