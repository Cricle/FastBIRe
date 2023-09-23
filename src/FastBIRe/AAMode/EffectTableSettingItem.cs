using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.AAMode
{
    public record class EffectTableSettingItem
    {
        public EffectTableSettingItem(DatabaseColumn effectColumn)
        {
            EffectColumn = effectColumn ?? throw new ArgumentNullException(nameof(effectColumn));
        }

        public DatabaseColumn EffectColumn { get; }
    }
}
