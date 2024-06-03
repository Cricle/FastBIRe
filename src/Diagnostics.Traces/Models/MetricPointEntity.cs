namespace Diagnostics.Traces.Models
{
    public record class MetricPointEntity
    {
        public double? Value { get; set; }

        public double? Sum { get; set; }

        public int? Count { get; set; }

        public double? Min { get; set; }

        public double? Max { get; set; }

        public List<MetricHistogramEntity>? Histograms { get; set; }

        public long? ZeroBucketCount { get; set; }

        public List<MetricBucketEntity>? Buckets { get; set; }

        public Dictionary<string, string>? Tags { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
