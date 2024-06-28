using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Diagnostics.Traces.Models
{
    [JsonSerializable(typeof(AcvtityEntity))]
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    public partial class AcvtityEntityJsonSerializerContext : JsonSerializerContext
    {

    }
    [JsonSerializable(typeof(AcvtityEntity))]
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    public partial class AcvtityEntityIgnoreNullJsonSerializerContext : JsonSerializerContext
    {

    }
    public record class AcvtityEntity : ITraceKeyProvider
    {
        public string? Id { get; set; }

        public ActivityStatusCode Status { get; set; }

        public string? StatusDescription { get; set; }

        public bool HasRemoteParent { get; set; }

        public ActivityKind Kind { get; set; }

        public string? OperationName { get; set; }

        public string? DisplayName { get; set; }

        public string? SourceName { get; set; }

        public string? SourceVersion { get; set; }

        public double Duration { get; set; }

        public DateTime StartTimeUtc { get; set; }

        public string? ParentId { get; set; }

        public string? RootId { get; set; }

        public Dictionary<string, string>? Tags { get; set; }

        public List<ActivityEventEntity>? Events { get; set; }

        public List<ActivityLinkEntity>? Links { get; set; }

        public Dictionary<string, string>? Baggage { get; set; }

        public ActivityLinkContextEntity? Context { get; set; }

        public string? TraceStateString { get; set; }

        public string? SpanId { get; set; }

        public string? TraceId { get; set; }

        public bool Recorded { get; set; }

        public ActivityTraceFlags ActivityTraceFlags { get; set; }

        public string? ParentSpanId { get; set; }

        public bool IsRootSpan()
        {
            return ParentSpanId == null || ParentSpanId == "0000000000000000";
        }
        public TraceKey GetTraceKey()
        {
            return new TraceKey(TraceId, SpanId);
        }
        public TraceKey GetParentTraceKey()
        {
            return new TraceKey(TraceId, ParentSpanId);
        }
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, AcvtityEntityJsonSerializerContext.Default.AcvtityEntity);
        }
    }
}
