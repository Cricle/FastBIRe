namespace FastBIRe
{
    public interface IDateTimePartNames
    {
        string Day { get; }
        string Hour { get; }
        string Minute { get; }
        string Month { get; }
        string Quarter { get; }
        string SystemPrefx { get; }
        string SystemSuffix { get; }
        string Week { get; }
        string Year { get; }

        string CombineField(string field, string part);
        IReadOnlyList<KeyValuePair<string, ToRawMethod>> GetDatePartNames(string field);
        bool TryGetField(ToRawMethod method, string field, out string? combinedField);
    }
}