namespace FastBIRe
{
    public class SyncIndexOptions
    {
        public string? Table { get; set; }

        public Func<string,string>? IndexNameCreator { get; set; }

        public IEnumerable<string>? Columns { get; set; }

        public bool RemoveNotRef { get; set; }

        public Func<string,bool>? RemoveFilter { get; set; }
    }
}
