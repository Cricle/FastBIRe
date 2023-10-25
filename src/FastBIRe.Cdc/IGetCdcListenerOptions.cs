using System.Collections.Generic;

namespace FastBIRe.Cdc
{
    public interface IGetCdcListenerOptions
    {
        IReadOnlyList<string>? TableNames { get; }
    }
}
