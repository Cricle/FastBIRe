using DatabaseSchemaReader;
using FastBIRe.Naming;
using FastBIRe.Store;
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

        protected virtual IEnumerable<string> GetTriggerScripts(DatabaseReader reader, EffectTriggerAAModelRequest request, string triggerName)
        {
            var triggerTypes = GetTriggerTypes();
            return TriggerWriter.CreateEffect(reader.SqlType!.Value,
                triggerName,
                triggerTypes,
                request.ArchiveTable.Name,
                request.EffectTable.Name,
                request.SettingItems,
                request.EffectTable.PrimaryKey != null && request.EffectTable.Columns.Any(x => x.IsAutoNumber && x.IsPrimaryKey));

        }

        protected override void AddTrigger(DatabaseReader reader, EffectTriggerAAModelRequest request, string triggerName, bool equals)
        {
            if (!equals)
            {
                var scripts = GetTriggerScripts(reader, request, triggerName);
                TriggerDataStore?.SetString(triggerName, string.Join("\n", scripts));
                request.AddScripts(scripts);
            }
        }
        protected override bool TriggerIsEquals(DatabaseReader reader, EffectTriggerAAModelRequest request, string triggerName)
        {
            if (TriggerDataStore != null)
            {
                var store = TriggerDataStore.GetString(triggerName);
                if (!string.IsNullOrEmpty(store) && SqlEqualityComparer != null)
                {
                    var genScripts = string.Join("\n", GetTriggerScripts(reader, request, triggerName));
                    var remoteScript = request.ArchiveTable.Triggers.First(x => x.Name == triggerName);
                    if (CheckRemote && !SqlEqualityComparer.Equals(store!, remoteScript.TriggerBody))
                    {
                        return false;
                    }
                    if (SqlEqualityComparer.Equals(store!, genScripts))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
