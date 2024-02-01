using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public interface IEventCounterProvider
    {
        bool AllNotNull { get; }

        event EventHandler? Changed;

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