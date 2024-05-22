using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Generator.Core
{
    public interface IOpetatorHandler<T>
    {
        Task HandleAsync(T input, CancellationToken token);
    }

    public interface IBatchOperatorHandler<T>
    {
        Task HandleAsync(BatchData<T> inputs, CancellationToken token);
    }
}
