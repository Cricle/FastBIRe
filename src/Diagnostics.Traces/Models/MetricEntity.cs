using OpenTelemetry.Metrics;

namespace Diagnostics.Traces.Models
{
    public class MetricEntity
    {
        public string? Name { get; set; }

        public string? Unit { get; set; }

        public MetricType MetricType { get; set; }

        public AggregationTemporality Temporality { get; set; }

        public string? Description { get; set; }

        public string? MeterName { get; set; }

        public string? MeterVersion { get; set; }

        public Dictionary<string, string>? MeterTags { get; set; }

        public DateTime CreateTime { get; set; }

        public List<MetricPointEntity>? Points { get; set; }

    }
}
