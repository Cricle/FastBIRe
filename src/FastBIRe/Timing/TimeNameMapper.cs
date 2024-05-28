namespace FastBIRe.Timing
{
    public class TimeNameMapper : ITimeNameMapper
    {
        public const string Year = "year";
        public const string Month = "month";
        public const string Day = "day";
        public const string Hour = "hour";
        public const string Minute = "minute";
        public const string Second = "second";
        public const string Quarter = "quarter";
        public const string Week = "week";

        public static readonly TimeNameMapper Instance = new TimeNameMapper();

        private TimeNameMapper() { }

        public string ToName(TimeTypes timeType)
        {
            switch (timeType)
            {
                case TimeTypes.Second:
                    return Second;
                case TimeTypes.Minute:
                    return Minute;
                case TimeTypes.Hour:
                    return Hour;
                case TimeTypes.Day:
                    return Day;
                case TimeTypes.Week:
                    return Week;
                case TimeTypes.Month:
                    return Month;
                case TimeTypes.Quarter:
                    return Quarter;
                case TimeTypes.Year:
                    return Year;
                case TimeTypes.None:
                default:
                    return string.Empty;
            }
        }
    }
}
