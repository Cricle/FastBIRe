using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
