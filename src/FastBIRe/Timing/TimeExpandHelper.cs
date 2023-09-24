using FastBIRe.Naming;

namespace FastBIRe.Timing
{
    public class TimeExpandHelper : ITimeExpandHelper
    {
        public TimeExpandHelper(INameGenerator nameGenerator, ITimeNameMapper timeNameMapper, FunctionMapper functionMapper)
        {
            NameGenerator = nameGenerator;
            TimeNameMapper = timeNameMapper;
            FunctionMapper = functionMapper;
        }

        public INameGenerator NameGenerator { get; }

        public ITimeNameMapper TimeNameMapper { get; }

        public FunctionMapper FunctionMapper { get; }

        public IEnumerable<TimeExpandResult> Create(string name, TimeTypes type)
        {
            var args = new string[2];
            args[0] = name;
            if ((type & TimeTypes.Second) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Second);
                var timeField = NameGenerator.Create(args);
                yield return new TimeExpandResult(TimeTypes.Second, timeField, name);
            }
            if ((type & TimeTypes.Minute) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Minute);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.MinuteFull(name);
                yield return new TimeExpandResult(TimeTypes.Minute, timeField, triggerExp);
            }
            if ((type & TimeTypes.Hour) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Hour);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.HourFull(name);
                yield return new TimeExpandResult(TimeTypes.Hour, timeField,triggerExp);
            }
            if ((type & TimeTypes.Day) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Day);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.DayFull(name);
                yield return new TimeExpandResult(TimeTypes.Day, timeField, triggerExp);
            }
            if ((type & TimeTypes.Week) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Week);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.WeekFull(name);
                yield return new TimeExpandResult(TimeTypes.Week, timeField, triggerExp);
            }
            if ((type & TimeTypes.Month) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Month);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.MonthFull(name);
                yield return new TimeExpandResult(TimeTypes.Month, timeField, triggerExp);
            }
            if ((type & TimeTypes.Quarter) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Quarter);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.QuarterFull(name);
                yield return new TimeExpandResult(TimeTypes.Quarter, timeField, triggerExp);
            }
            if ((type & TimeTypes.Year) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Year);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.YearFull(name);
                yield return new TimeExpandResult(TimeTypes.Year, timeField, triggerExp);
            }
        }
    }
}
