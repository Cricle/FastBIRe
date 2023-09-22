using System.Runtime.CompilerServices;

namespace FastBIRe.Timescale
{
    public partial class TimescaleHelper
    {
        public const string Year = "year";
        public const string Month = "month";
        public const string Day = "day";
        public const string Hour = "hour";
        public const string Minute = "minute";
        public const string Second = "second";
        public const string Quarter = "quarter";

        public static readonly TimescaleHelper Default = new TimescaleHelper();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string BoolToString(bool? b)
        {
            return b ?? false ? "true" : "false";
        }

        public string CreateInterval(int value, string unit)
        {
            var unitStr = string.Empty;
            if (unit==Quarter)
            {
                value *= 3;
            }
            return $"INTERVAL '{value} {unitStr}'";
        }
    }
}
