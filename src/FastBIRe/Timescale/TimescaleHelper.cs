using System.Runtime.CompilerServices;

namespace FastBIRe.Timescale
{
    public partial class TimescaleHelper
    {
        public static readonly TimescaleHelper Default = new TimescaleHelper();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string BoolToString(bool? b)
        {
            return b ?? false ? "true" : "false";
        }

        public string CreateInterval(int value, DateTimeUnit unit)
        {
            var unitStr = string.Empty;
            switch (unit)
            {
                case DateTimeUnit.Year:
                    unitStr = "year";
                    break;
                case DateTimeUnit.Month:
                    unitStr = "month";
                    break;
                case DateTimeUnit.Day:
                    unitStr = "day";
                    break;
                case DateTimeUnit.Hour:
                    unitStr = "hour";
                    break;
                case DateTimeUnit.Minute:
                    unitStr = "minute";
                    break;
                case DateTimeUnit.Second:
                    unitStr = "second";
                    break;
                case DateTimeUnit.Week:
                    unitStr = "week";
                    break;
                case DateTimeUnit.Quarter:
                    value *= 3;
                    unitStr = "months";
                    break;
                default:
                    break;
            }
            return $"{value} {unitStr}";
        }
    }
}
