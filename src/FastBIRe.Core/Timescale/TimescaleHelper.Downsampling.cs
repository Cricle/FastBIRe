namespace FastBIRe.Timescale
{
    public static partial class TimescaleHelper
    {
        public static string AsapMmooth(string ts, string value, string resolution)
        {
            return $"asap_smooth({ts},{value},{resolution})";
        }
        public static string GpLttb(string ts, string value, string resolution, string? gapsize = null)
        {
            var gapsizeStr = gapsize == null ? string.Empty : "," + gapsize;

            return $"gp_lttb({ts},{value},{resolution}{gapsizeStr})";
        }
        public static string Lttb(string ts, string value, string resolution)
        {
            return $"lttb({ts},{value},{resolution})";
        }
    }
}
