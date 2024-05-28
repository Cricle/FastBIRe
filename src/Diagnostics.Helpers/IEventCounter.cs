using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public interface IEventCounterProvider
    {
        IEnumerable<string> EventNames { get; }

        bool AllNotNull { get; }

        event EventHandler? Changed;

        bool TryGetCounterPayload(string name, out ICounterPayload? payload);

        void Reset();
        void Update(ICounterPayload payload);
        void WriteTo(TextWriter writer);
        Task OnceAsync(CancellationToken token = default);
    }
    public interface IEventCounter<TCounter> : IEventCounterProvider
    {
        TCounter Copy();
    }
    public static class EventCounterOnceExtensions
    {
        public static IEnumerable<KeyValuePair<string,ICounterPayload?>> EnumerablePlayload(this IEventCounterProvider counter,bool includeEmpty=true)
        {
            foreach (var item in counter.EventNames)
            {
                if (!counter.TryGetCounterPayload(item,out var playload)&&!includeEmpty)
                {
                    continue;
                }
                yield return new KeyValuePair<string, ICounterPayload?>(item, playload);
            }
        }
        public static async Task<TCounter> OnceAndReturnAsync<TCounter>(this TCounter counter,CancellationToken token = default)
            where TCounter : IEventCounter<TCounter>
        {
            await counter.OnceAsync(token);
            return counter;
        }
        public static async Task OnceAsync<TCounter>(this TCounter counter, Action<TCounter> action, CancellationToken token = default)
            where TCounter : IEventCounter<TCounter>
        {
            await counter.OnceAsync(token);
            action(counter);
        }
    }
}