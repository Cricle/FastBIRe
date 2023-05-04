using System.Globalization;

namespace FastBIRe
{
    public class DefaultSpliteStrategyTablePartConverter : ISpliteStrategyTablePartConverter
    {
        public const string NullString = "null";

        public static readonly DefaultSpliteStrategyTablePartConverter Year = new DefaultSpliteStrategyTablePartConverter(ToRawMethod.Year);
        public static readonly DefaultSpliteStrategyTablePartConverter Month = new DefaultSpliteStrategyTablePartConverter(ToRawMethod.Month);
        public static readonly DefaultSpliteStrategyTablePartConverter Day = new DefaultSpliteStrategyTablePartConverter(ToRawMethod.Day);

        public DefaultSpliteStrategyTablePartConverter(ToRawMethod method)
            : this(method, CultureInfo.CurrentCulture.Calendar, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
        {

        }
        public DefaultSpliteStrategyTablePartConverter(ToRawMethod method, Calendar calendar)
            :this(method,calendar, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
        {

        }
        public DefaultSpliteStrategyTablePartConverter(ToRawMethod method, Calendar calendar, CalendarWeekRule weekRule, DayOfWeek dayOfWeek)
        {
            Method = method;
            Calendar = calendar;
            WeekRule = weekRule;
            DayOfWeek = dayOfWeek;
        }

        public ToRawMethod Method { get; }

        public Calendar Calendar { get; }

        public CalendarWeekRule WeekRule { get; }

        public DayOfWeek DayOfWeek { get; }

        public string? NullValue { get; set; }

        public virtual string? Convert(object value)
        {
            if (value is null)
            {
                return NullValue;
            }
            if (value is DateTime time)
            {
                switch (Method)
                {
                    case ToRawMethod.Year:
                        return time.ToString("yyyy");
                    case ToRawMethod.Month:
                        return time.ToString("yyyy-MM");
                    case ToRawMethod.Quarter:
                        var quarter= (time.Month - 1) / 3 + 1;
                        return $"{time.Year}-{quarter}";
                    case ToRawMethod.Weak:
                        var weak=Calendar.GetWeekOfYear(time, WeekRule, DayOfWeek);
                        return $"{time.Year}-{weak}";
                    case ToRawMethod.Day:
                    default:
                        return time.ToString("yyyy-MM-dd");
                }
            }
            return value.ToString();
        }
    }
}
