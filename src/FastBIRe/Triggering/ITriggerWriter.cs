namespace FastBIRe.Triggering
{
    public interface ITriggerWriter
    {
        IEnumerable<string> Create(FSqlType sqlType, string name, TriggerTypes type, string table, string body, string? when);

        IEnumerable<string> Drop(FSqlType sqlType, string name, string table);

        string GetTriggerName(TriggerTypes type, FSqlType sqlType);
    }
}