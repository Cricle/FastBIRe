using FastBIRe.Naming;
using FastBIRe.Triggering;

namespace FastBIRe.AAMode
{
    public class EffectInsertTriggerAAModelHelper : EffectTriggerAAModelHelper
    {
        public EffectInsertTriggerAAModelHelper(INameGenerator triggerNameGenerator, ITriggerWriter triggerWriter) : base(triggerNameGenerator, triggerWriter)
        {
        }

        protected override TriggerTypes GetTriggerTypes()
        {
            return TriggerTypes.BeforeInsert;
        }
    }
}
