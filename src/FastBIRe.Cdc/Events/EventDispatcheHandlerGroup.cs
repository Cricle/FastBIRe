using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.Events
{
    public class EventDispatcheHandlerGroup<TInput> : List<IEventDispatcheHandler<TInput>>, IEventDispatcheHandler<TInput>
    {
        public async Task HandleAsync(TInput input, CancellationToken token = default)
        {
            foreach (var item in this)
            {
                await item.HandleAsync(input, token);
            }
        }
    }
}
