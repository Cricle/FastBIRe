using FastBIRe.Wrapping;
using System.Data;
using System.Data.Common;
using System.Text;

namespace FastBIRe.Data
{
    public interface IMirrorCopy<TResult>
    {
        Task<IList<TResult>> CopyAsync(CancellationToken token=default);
    }
}
