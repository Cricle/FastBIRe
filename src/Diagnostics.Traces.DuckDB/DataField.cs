namespace Diagnostics.Traces.DuckDB
{
    internal readonly record struct DataField<T>(string Name, string Type,T Mode)
    {
        public override string ToString()
        {
            return $"{Name} {Type}";
        }
    }
}
