using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc
{
    public interface ICdcListenerOptionCreator
    {
        Task<ICdcListener> CreateCdcListnerAsync(in CdcListenerOptionCreateInfo info, CancellationToken token = default);
    }
}
