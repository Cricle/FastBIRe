namespace Diagnostics.Traces.Models
{
    public record struct ActivityEventEntity
    {
        public string? Name { get; set; }

        public DateTime Timestamp { get; set; }

        public Dictionary<string, string?>? Tags { get; set; }
    }
}
