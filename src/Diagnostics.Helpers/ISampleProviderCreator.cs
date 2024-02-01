using System;

namespace Diagnostics.Helpers
{
    public interface ISampleProviderCreator
    {
        ISampleProvider GetSample(ICounterResult counterResult);

        ISampleProvider GetIntervalSample(ICounterResult counterResult, TimeSpan interval);
    }
}
