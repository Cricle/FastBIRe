using System.Collections.Generic;

namespace FastBIRe.Cdc
{
    public interface ICdcLog : IDictionary<string, object>
    {
        string Name { get; }

        ulong? Length { get; }
    }
}
