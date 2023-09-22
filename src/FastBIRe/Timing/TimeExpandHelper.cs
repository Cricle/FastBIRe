using FastBIRe.AAMode;
using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe.Timing
{
    public interface ITimeNameMapper
    {
        string ToName(TimeTypes timeType);
    }
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
    public class TimeExpandHelper
    {
        public TimeExpandHelper(INameGenerator nameGenerator, ITimeNameMapper timeNameMapper)
        {
            NameGenerator = nameGenerator;
            TimeNameMapper = timeNameMapper;
        }

        public INameGenerator NameGenerator { get; }

        public ITimeNameMapper TimeNameMapper { get; }

        public IEnumerable<string> Create(string name, TimeTypes type)
        {
            var args = new string[2];
            args[0] = name;
            if ((type & TimeTypes.Second) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Second);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Minute) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Minute);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Hour) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Hour);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Day) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Day);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Week) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Week);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Month) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Month);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Quarter) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Quarter);
                yield return NameGenerator.Create(args);
            }
            if ((type & TimeTypes.Year) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Year);
                yield return NameGenerator.Create(args);
            }
        }
    }
}
