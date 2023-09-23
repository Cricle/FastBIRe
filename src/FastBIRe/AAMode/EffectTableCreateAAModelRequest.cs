using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.AAMode
{
    public class EffectTableCreateAAModelRequest : AAModeRequest
    {
        public EffectTableCreateAAModelRequest(DatabaseTable archiveTable, DatabaseTable aggregationTable, IReadOnlyList<EffectTableSettingItem> settingItems)
            : base(archiveTable, aggregationTable)
        {
            SettingItems = settingItems;
        }

        public IReadOnlyList<EffectTableSettingItem> SettingItems { get; }
    }
}
