using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.AAMode
{
    public interface IModeHelper<in TModeRequest>
    {
        void Apply(DatabaseReader reader, TModeRequest request);
    }
    public class ModeHelperGroup<TModeRequest> : List<IModeHelper<TModeRequest>>, IModeHelper<TModeRequest>
    {
        public void Apply(DatabaseReader reader, TModeRequest request)
        {
            foreach (var item in this)
            {
                item.Apply(reader, request);
            }
        }
    }
    public class ScriptingRequest
    {
        public ScriptingRequest()
        {
            Scripts = CreateScripts();
            lstScripts = Scripts as List<string>;
        }
        private readonly List<string>? lstScripts;

        public virtual IList<string> Scripts { get; }

        protected virtual IList<string> CreateScripts()
        {
            return new List<string>();
        }

        public void AddScripts(IEnumerable<string> scripts)
        {
            if (lstScripts!=null)
            {
                lstScripts.AddRange(scripts);
            }
            else
            {
                foreach (var item in scripts)
                {
                    Scripts.Add(item);
                }
            }
        }
    }
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
