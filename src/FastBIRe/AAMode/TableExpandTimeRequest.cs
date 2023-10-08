using FastBIRe.Timing;

namespace FastBIRe.AAMode
{
    public class TableExpandTimeRequest : ScriptingRequest
    {
        public TableExpandTimeRequest(string tableName, IEnumerable<string> columns, TimeTypes timeTypes = TimeTypes.ExceptSecond)
        {
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
            TimeTypes = timeTypes;
            if ((timeTypes & TimeTypes.All) == TimeTypes.None)
            {
                throw new ArgumentException($"Time type must not none");
            }
        }

        public string TableName { get; }

        public IEnumerable<string> Columns { get; }

        public TimeTypes TimeTypes { get; }
    }
}
