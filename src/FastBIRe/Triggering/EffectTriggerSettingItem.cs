using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Triggering
{
    public record class EffectTriggerSettingItem : FieldRaw
    {
        public EffectTriggerSettingItem(string field, string raw, string rawFormat)
            : base(field, raw, rawFormat)
        {
        }

        protected EffectTriggerSettingItem(FieldRaw original)
            : base(original)
        {
        }

        public static EffectTriggerSettingItem Trigger(string field, SqlType sqlType)
        {
            var qutoName = sqlType.Wrap(field);
            var triggerField = $"NEW.{qutoName}";
            return new EffectTriggerSettingItem(field, triggerField, qutoName);
        }
    }
}
