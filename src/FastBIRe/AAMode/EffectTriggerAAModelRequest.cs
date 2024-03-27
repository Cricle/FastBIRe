using DatabaseSchemaReader.DataSchema;
using FastBIRe.Triggering;

namespace FastBIRe.AAMode
{
    public class EffectTriggerAAModelRequest : AAModeRequest
    {
        public EffectTriggerAAModelRequest(DatabaseTable archiveTable, DatabaseTable aggregationTable, DatabaseTable effectTable)
            : base(archiveTable, aggregationTable)
        {
            SettingItems = new List<EffectTriggerSettingItem>();
            EffectTable = effectTable;
        }

        public DatabaseTable EffectTable { get; }

        public List<EffectTriggerSettingItem> SettingItems { get; }
    }
}
