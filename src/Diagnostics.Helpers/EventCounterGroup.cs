using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public class EventCounterGroup : IReadOnlyList<IEventCounterProvider>, IEventCounter<EventCounterGroup>
    {
        public EventCounterGroup()
        {
            providers = new List<IEventCounterProvider>();
        }
        public EventCounterGroup(IEnumerable<IEventCounterProvider> providers)
        {
            this.providers = providers.ToList();
            for (int i = 0; i < this.providers.Count; i++)
            {
                this.providers[i].Changed += OnProviderChanged;
            }
        }

        private readonly IList<IEventCounterProvider> providers;

        public IEventCounterProvider this[int index] => providers[index];

        public int Count => providers.Count;

        public bool AllNotNull
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    if (!this[i].AllNotNull)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public IEnumerable<string> EventNames => this.SelectMany(x => x.EventNames).Distinct();

        public int EventNameCount => EventNames.Count();

        public event EventHandler? Changed;

        public IEnumerator<IEventCounterProvider> GetEnumerator()
        {
            return providers.GetEnumerator();
        }

        public void Add(IEventCounterProvider provider)
        {
            providers.Add(provider);
            provider.Changed += OnProviderChanged;
        }
        public void Remove(IEventCounterProvider provider)
        {
            if (providers.Remove(provider))
            {
                provider.Changed -= OnProviderChanged;
            }
        }
        public void Clear()
        {
            while (Count > 0)
            {
                this[0].Changed -= OnProviderChanged;
                providers.RemoveAt(0);
            }
        }

        private void OnProviderChanged(object? sender, EventArgs e)
        {
            Changed?.Invoke(sender, e);
        }

        public void Reset()
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].Reset();
            }
        }

        public void Update(ICounterPayload payload)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].Update(payload);
            }
        }

        public void WriteTo(TextWriter writer)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].WriteTo(writer);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public EventCounterGroup Copy()
        {
            return new EventCounterGroup(providers);
        }

        public async Task<EventCounterGroup> OnceAsync(CancellationToken token = default)
        {
            for (int i = 0; i < Count; i++)
            {
                await this[i].OnceAsync(token);
            }
            return this;
        }

        public async Task OnceAsync(Action<EventCounterGroup> action, CancellationToken token = default)
        {
            var group = await OnceAsync(token);
            action(group);
        }

        async Task IEventCounterProvider.OnceAsync(CancellationToken token)
        {
            for (int i = 0; i < Count; i++)
            {
                await this[i].OnceAsync(token);
            }
        }

        public bool TryGetCounterPayload(string name, out ICounterPayload? payload)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].TryGetCounterPayload(name,out payload))
                {
                    return true;
                }
            }
            payload = null;
            return false;
        }
    }
}