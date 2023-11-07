using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.Events
{
    public interface IEventDispatcheHandler<TInput>
    {
        Task HandleAsync(TInput input, CancellationToken token = default);
    }
}
