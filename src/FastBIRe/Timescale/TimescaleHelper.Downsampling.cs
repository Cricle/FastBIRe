using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe.Timescale
{
    public partial class TimescaleHelper
    {
        public string AsapMmooth(string ts,string value, string resolution)
        {
            return $"asap_smooth({ts},{value},{resolution})";
        }
        public string GpLttb(string ts, string value, string resolution, string? gapsize = null)
        {
            var gapsizeStr = gapsize == null ? string.Empty : "," + gapsize;

            return $"gp_lttb({ts},{value},{resolution}{gapsizeStr})";
        }
        public string Lttb(string ts, string value, string resolution)
        {
            return $"lttb({ts},{value},{resolution})";
        }
    }
}
