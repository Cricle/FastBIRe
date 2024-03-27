#nullable disable
using Microsoft.Diagnostics.Runtime;

namespace Diagnostics.Helpers.Analyzer
{
    public sealed record class HeapInfo
    {
        public int Index;
        public GenerationInfo Ephemeral = new();
        public GenerationInfo Gen0 = new();
        public GenerationInfo Gen1 = new();
        public GenerationInfo Gen2 = new();
        public GenerationInfo LoH = new();
        public GenerationInfo PoH = new();
        public GenerationInfo Frozen = new();

        public static HeapInfo operator +(HeapInfo left, HeapInfo right)
        {
            return new()
            {
                Index = -1,
                Ephemeral = left.Ephemeral + right.Ephemeral,
                Gen0 = left.Gen0 + right.Gen0,
                Gen1 = left.Gen1 + right.Gen1,
                Gen2 = left.Gen2 + right.Gen2,
                LoH = left.LoH + right.LoH,
                PoH = left.PoH + right.PoH,
                Frozen = left.Frozen + right.Frozen,
            };
        }

        public GenerationInfo GetInfoByGeneration(Generation gen)
        {
            return gen switch
            {
                Generation.Generation0 => Gen0,
                Generation.Generation1 => Gen1,
                Generation.Generation2 => Gen2,
                Generation.Large => LoH,
                Generation.Pinned => PoH,
                Generation.Frozen => Frozen,
                _ => null
            };
        }
    }
}
#nullable restore