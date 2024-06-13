using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Diagnostics.Traces
{
    public class ActivityJsonConverter : JsonConverter<Activity>
    {
        public const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static readonly ActivityJsonConverter Instance = new ActivityJsonConverter();

        public override Activity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
        public static void Write(Utf8JsonWriter writer, Activity value)
        {
            writer.WriteStartObject();

            writer.WriteString("Id", value.Id);

            writer.WriteString("Status", value.Status.ToString());
            writer.WriteString("StatusDescription", value.StatusDescription);
            writer.WriteBoolean("HasRemoteParent", value.HasRemoteParent);
            writer.WriteString("Kind", value.Kind.ToString());
            writer.WriteString("OperationName", value.OperationName);
            writer.WriteString("DisplayName", value.DisplayName);

            writer.WriteStartObject("Source");
            writer.WriteString("Name", value.Source.Name);
            writer.WriteString("Version", value.Source.Version);
            writer.WriteEndObject();

            writer.WriteString("Duration", value.Duration.ToString());
            writer.WriteString("StartTimeUtc", value.StartTimeUtc.ToLocalTime().ToString(TimeFormat));

            writer.WriteString("ParentId", value.ParentId);
            writer.WriteString("RootId", value.RootId);

            writer.WriteStartArray("Tags");
            foreach (var tag in value.Tags)
            {
                writer.WriteStartObject();
                writer.WriteString("Key", tag.Key);
                writer.WriteString("Value", tag.Value);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteStartArray("Events");
            foreach (var activityEvent in value.Events)
            {
                writer.WriteStartObject();
                writer.WriteString("Name", activityEvent.Name);
                writer.WriteString("Timestamp", activityEvent.Timestamp.ToLocalTime().ToString(TimeFormat));

                writer.WriteStartArray("Tags");
                foreach (var tag in activityEvent.Tags)
                {
                    writer.WriteStartObject();
                    writer.WriteString("Key", tag.Key);
                    writer.WriteString("Value", tag.Value?.ToString());
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteStartArray("Links");
            foreach (var link in value.Links)
            {
                writer.WriteStartObject();

                writer.WriteStartObject("Context");
                writer.WriteString("TraceId", link.Context.TraceId.ToString());
                writer.WriteString("TraceState", link.Context.TraceState);
                writer.WriteString("TraceFlags", link.Context.TraceFlags.ToString());
                writer.WriteBoolean("IsRemote", link.Context.IsRemote);
                writer.WriteString("SpanId", link.Context.SpanId.ToString());
                writer.WriteEndObject();

                writer.WriteStartObject("Tags");
                if (link.Tags != null)
                {
                    foreach (var item in link.Tags)
                    {
                        writer.WriteString(item.Key, item.Value?.ToString());
                    }
                }
                writer.WriteEndObject();

                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteStartArray("Baggage");
            foreach (var baggageItem in value.Baggage)
            {
                writer.WriteStartObject();
                writer.WriteString("Key", baggageItem.Key);
                writer.WriteString("Value", baggageItem.Value);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteStartObject("Context");
            writer.WriteString("TraceId", value.Context.TraceId.ToString());
            writer.WriteString("SpanId", value.Context.SpanId.ToString());
            writer.WriteString("TraceFlags", value.Context.TraceFlags.ToString());
            writer.WriteBoolean("IsRemote", value.Context.IsRemote);
            writer.WriteEndObject();

            writer.WriteString("TraceStateString", value.TraceStateString);
            writer.WriteString("SpanId", value.SpanId.ToString());
            writer.WriteString("TraceId", value.TraceId.ToString());
            writer.WriteBoolean("Recorded", value.Recorded);
            writer.WriteBoolean("IsAllDataRequested", value.IsAllDataRequested);
            writer.WriteString("ActivityTraceFlags", value.ActivityTraceFlags.ToString());
            writer.WriteString("ParentSpanId", value.ParentSpanId.ToString());
            writer.WriteBoolean("IsStopped", value.IsStopped);
            writer.WriteString("IdFormat", value.IdFormat.ToString());

            writer.WriteEndObject();
        }

        public override void Write(Utf8JsonWriter writer, Activity value, JsonSerializerOptions options)
        {
            Write(writer, value);
        }
    }
}
