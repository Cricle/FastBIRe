using DatabaseSchemaReader;
using DatabaseSchemaReader.Data;
using DatabaseSchemaReader.DataSchema;
using FastBIRe.Naming;
using FastBIRe.Store;
using FastBIRe.Triggering;

namespace FastBIRe.AAMode
{
    public abstract class TriggerAAModelHelper<TModelRequest> : IModeHelper<TModelRequest>
        where TModelRequest : AAModeRequest
    {
        protected TriggerAAModelHelper(INameGenerator triggerNameGenerator, ITriggerWriter triggerWriter)
        {
            TriggerNameGenerator = triggerNameGenerator;
            TriggerWriter = triggerWriter;
        }

        public INameGenerator TriggerNameGenerator { get; }

        public ITriggerWriter TriggerWriter { get; }

        public IDataStore? TriggerDataStore { get; set; }

        public IEqualityComparer<string>? SqlEqualityComparer { get; set; }

        public bool CheckRemote { get; set; }

        public bool OnlyDrop { get; set; }

        public void Apply(DatabaseReader reader, TModelRequest request)
        {
            var triggerName = GetTriggerName(reader, request);
            var exists = IsTriggerExists(reader, request, triggerName);
            var equals = exists && TriggerIsEquals(reader, request, triggerName);
            if (exists && (!equals || OnlyDrop))
            {
                var dropSqls = TriggerWriter.Drop(reader.SqlType!.Value, triggerName, request.ArchiveTable.Name);
                request.AddScripts(dropSqls);
            }
            if (OnlyDrop)
            {
                AddTrigger(reader, request, triggerName, equals);
            }
        }

        protected abstract void AddTrigger(DatabaseReader reader, TModelRequest request, string triggerName, bool triggerIsEquals);

        protected virtual string GetTriggerName(DatabaseReader reader, TModelRequest request)
        {
            return TriggerNameGenerator.Create(new[] { request.ArchiveTable.Name });
        }
        protected virtual bool TriggerIsEquals(DatabaseReader reader, TModelRequest request, string triggerName)
        {
            return false;
        }
        protected virtual bool IsTriggerExists(DatabaseReader reader, TModelRequest request, string triggerName)
        {
            return request.ArchiveTable.Triggers.Any(x => x.Name == triggerName);
        }
    }
}
