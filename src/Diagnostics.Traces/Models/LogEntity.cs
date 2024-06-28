using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Diagnostics.Traces.Models
{
    public interface ITraceKeyProvider
    {
        TraceKey GetTraceKey();
    }
    [JsonSerializable(typeof(LogEntity))]
    public partial class LogEntityJsonSerializerContext : JsonSerializerContext
    {

    }
    public class LogEntity: ITraceKeyProvider
    {
        public DateTime Timestamp { get; set; }

        public LogLevel LogLevel { get; set; }

        public string? CategoryName { get; set; }

        public string? TraceId { get; set; }

        public string? SpanId { get; set; }

        public Dictionary<string, string>? Attributes { get; set; }

        public string? FormattedMessage { get; set; }

        public string? Body { get; set; }

        public TraceKey GetTraceKey()
        {
            return new TraceKey(TraceId, SpanId);
        }
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, LogEntityJsonSerializerContext.Default.LogEntity);
        }
    }
}
