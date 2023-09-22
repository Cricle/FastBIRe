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
    }
}
