using System;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public interface ISampleResult<TCounter> : ISampleProvider
        where TCounter : IEventCounter<TCounter>
    {
        new TCounter Counter { get; }
    }
}
