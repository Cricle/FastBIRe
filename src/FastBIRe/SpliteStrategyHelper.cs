namespace FastBIRe
{
    public static class SpliteStrategyHelper
    {
        public static IEnumerable<object> GetStrategyValues(IEnumerable<int> indexs, IReadOnlyList<object> values, int offset)
        {
            foreach (var index in indexs)
            {
                yield return values[offset + index];
            }
        }
    }
}
