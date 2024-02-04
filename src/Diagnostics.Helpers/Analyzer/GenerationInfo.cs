#nullable disable
namespace Diagnostics.Helpers.Analyzer
{
    public sealed record class GenerationInfo
    {
        public ulong Allocated;
        public ulong Free;
        public ulong Unrooted;
        public ulong Committed;

        public static GenerationInfo operator +(GenerationInfo left, GenerationInfo right)
        {
            return new()
            {
                Allocated = left.Allocated + right.Allocated,
                Free = left.Free + right.Free,
                Unrooted = left.Unrooted + right.Unrooted,
                Committed = left.Committed + right.Committed
            };
        }
    }
}
#nullable restore