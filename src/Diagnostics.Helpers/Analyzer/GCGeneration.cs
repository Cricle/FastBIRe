#nullable disable
namespace Diagnostics.Helpers.Analyzer
{
    public enum GCGeneration
    {
        NotSet = 0,
        Generation0 = 1,
        Generation1 = 2,
        Generation2 = 3,
        LargeObjectHeap = 4,
        PinnedObjectHeap = 5,
        FrozenObjectHeap = 6
    }
}
#nullable restore