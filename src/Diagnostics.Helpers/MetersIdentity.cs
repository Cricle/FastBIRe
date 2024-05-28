using Diagnostics.Helpers;

namespace Tracker
{
    public readonly struct MetersIdentity
    {
        public MetersIdentity(IEventSampleCreator eventSampleCreator,string name)
        {
            Name = name;
            EventSampleCreator = eventSampleCreator;
        }

        public string Name { get; }

        public IEventSampleCreator EventSampleCreator { get; }
    }
}
