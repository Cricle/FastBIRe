using FastBIRe.Naming;
using FastBIRe.Triggering;

namespace FastBIRe.AAMode
{
    public class EffectUpdateTriggerAAModelHelper : EffectTriggerAAModelHelper
    {
        public EffectUpdateTriggerAAModelHelper(INameGenerator triggerNameGenerator, ITriggerWriter triggerWriter) : base(triggerNameGenerator, triggerWriter)
        {
        }

        protected override TriggerTypes GetTriggerTypes()
        {
            return TriggerTypes.BeforeUpdate;
        }
    }
}
