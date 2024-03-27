using System;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public static class SampleResultExtensions
    {
        public static async Task OnceAsync<TCounter>(this ISampleResult<TCounter> result, Action<TCounter> action, CancellationToken token)
            where TCounter : IEventCounter<TCounter>
        {
            await result.OnceAsync(token);
            action(result.Counter);
        }
    }
}
