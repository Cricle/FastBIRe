using System;
using System.Collections.Generic;

namespace FastBIRe.Cdc
{
    public class CdcLog : Dictionary<string, object>, ICdcLog
    {
        public CdcLog(string name, ulong? length)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            Name = name;
            Length = length;
        }

        public string Name { get; }

        public ulong? Length { get; }
    }
}
