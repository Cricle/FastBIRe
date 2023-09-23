using DatabaseSchemaReader;
using FastBIRe.Naming;
using FastBIRe.Triggering;

namespace FastBIRe.AAMode
{
    public abstract class EffectTriggerAAModelHelper : TriggerAAModelHelper<EffectTriggerAAModelRequest>
    {
        public EffectTriggerAAModelHelper(INameGenerator triggerNameGenerator, ITriggerWriter triggerWriter)
            : base(triggerNameGenerator, triggerWriter)
        {
        }
        protected abstract TriggerTypes GetTriggerTypes();

        protected override void AddTrigger(DatabaseReader reader, EffectTriggerAAModelRequest request, string triggerName)
        {
            var triggerTypes = GetTriggerTypes();
            var sqls = TriggerWriter.CreateEffect(reader.SqlType!.Value,
                triggerName,
                triggerTypes,
                request.ArchiveTable.Name,
                request.EffectTable.Name,
                request.SettingItems);
            request.AddScripts(sqls);
        }
    }
}
