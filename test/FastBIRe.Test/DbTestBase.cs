using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBIRe.Test
{
    public abstract class DbTestBase
    {
        public string Quto(SqlType type,string name)
        {
            return MergeHelper.Wrap(type, name);
        }
    }
}
