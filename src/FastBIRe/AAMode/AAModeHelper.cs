using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.AAMode
{
    public class AAModeRequest : ScriptingRequest
    {
        public AAModeRequest(DatabaseTable archiveTable, DatabaseTable aggregationTable)
        {
            ArchiveTable = archiveTable;
            AggregationTable = aggregationTable;
        }

        public DatabaseTable ArchiveTable { get; }

        public DatabaseTable AggregationTable { get; }
    }
    public class AAModeHelper : IModeHelper<AAModeRequest>
    {
        public virtual void Apply(DatabaseReader reader, AAModeRequest request)
        {

        }
    }
}
