namespace Diagnostics.Traces.Models
{
    public record struct MetricHistogramEntity
    {
        public double RangeLeft { get; set; }

        public double RangeRight { get; set; }

        public int BucketCount { get; set; }
    }
}
