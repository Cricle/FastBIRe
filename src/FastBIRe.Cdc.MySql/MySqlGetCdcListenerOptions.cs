using System.Collections.Generic;

namespace FastBIRe.Cdc.MySql
{
    public class MySqlGetCdcListenerOptions : IGetCdcListenerOptions
    {
        public MySqlGetCdcListenerOptions(IReadOnlyList<string>? tableNames)
        {
            TableNames = tableNames;
        }

        public IReadOnlyList<string>? TableNames { get; }
    }
}
