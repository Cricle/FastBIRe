using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Triggering
{
    public interface ITriggerWriter
    {
        IEnumerable<string> Create(SqlType sqlType, string name, TriggerTypes type, string table, string body, string? when);

        IEnumerable<string> Drop(SqlType sqlType, string name,string table);

        string GetTriggerName(TriggerTypes type, SqlType sqlType);
    }
}