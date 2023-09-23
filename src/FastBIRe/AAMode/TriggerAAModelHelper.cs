using DatabaseSchemaReader;
using FastBIRe.Naming;
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

        public void Apply(DatabaseReader reader, TModelRequest request)
        {
            var triggerName = GetTriggerName(reader, request);
            if (IsTriggerExists(reader,request,triggerName))
            {
                var dropSqls = TriggerWriter.Drop(reader.SqlType!.Value, triggerName);
                request.AddScripts(dropSqls);
            }
            AddTrigger(reader, request, triggerName);
        }

        protected abstract void AddTrigger(DatabaseReader reader, TModelRequest request,string triggerName);

        protected virtual string GetTriggerName(DatabaseReader reader, TModelRequest request)
        {
            return TriggerNameGenerator.Create(new[] { request.ArchiveTable.Name });
        }

        protected virtual bool IsTriggerExists(DatabaseReader reader, TModelRequest request,string triggerName)
        {
            return request.ArchiveTable.Triggers.Any(x => x.Name == triggerName);
        }
    }
}
