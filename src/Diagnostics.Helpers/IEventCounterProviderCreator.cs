using System;

namespace Diagnostics.Helpers
{
    public interface IEventCounterProviderCreator
    {
        bool SupportIntervalCounterProvider { get; }

        IEventCounterProvider CreateCounterProvider();

        IEventCounterProvider CreateIntervalCounterProvider(TimeSpan interval);
    }
}
