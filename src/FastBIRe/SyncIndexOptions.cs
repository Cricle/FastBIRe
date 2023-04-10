namespace FastBIRe
{
    public class SyncIndexOptions
    {
        public string? Table { get; set; }

        public string? IndexName { get; set; }

        public IEnumerable<string>? Columns { get; set; }

        public bool DuplicationNameReplace { get; set; }

        public bool IgnoreOnSequenceEqualName { get; set; } = true;

        public Func<string, string>? IndexNameCreator { get; set; }

        public bool RemoveNotRef { get; set; }

        public Func<string, bool>? RemoveFilter { get; set; }
    }
}
