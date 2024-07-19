namespace Diagnostics.Traces.Models
{
    public record struct ActivityLinkEntity
    {
        public ActivityLinkContextEntity Context { get; set; }

        public Dictionary<string, string?>? Tags { get; set; }
    }
}
