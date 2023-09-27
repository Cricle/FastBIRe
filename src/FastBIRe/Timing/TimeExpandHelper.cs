using DatabaseSchemaReader.DataSchema;
using FastBIRe.Naming;

namespace FastBIRe.Timing
{
    public class TimeExpandHelper : ITimeExpandHelper
    {
        public static readonly INameGenerator DefaultNameGenerator = new RegexNameGenerator("__${0}_{1}");

        public static readonly TimeExpandHelper MySqlDefault = new TimeExpandHelper(SqlType.MySql);
        public static readonly TimeExpandHelper SqlServerDefault = new TimeExpandHelper(SqlType.SqlServer);
        public static readonly TimeExpandHelper SqliteDefault = new TimeExpandHelper(SqlType.SQLite);
        public static readonly TimeExpandHelper PostgresqlDefault = new TimeExpandHelper(SqlType.PostgreSql);

        public static TimeExpandHelper? GetDefault(SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return SqlServerDefault;
                case SqlType.MySql:
                    return MySqlDefault;
                case SqlType.SQLite:
                    return SqliteDefault;
                case SqlType.PostgreSql:
                    return PostgresqlDefault;
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    return null;
            }
        }

        public TimeExpandHelper(SqlType sqlType)
            : this(FunctionMapper.Get(sqlType) ?? throw new NotSupportedException(sqlType.ToString()))
        {

        }
        public TimeExpandHelper(FunctionMapper functionMapper)
            :this(DefaultNameGenerator,Timing.TimeNameMapper.Instance,functionMapper)
        {

        }

        public TimeExpandHelper(INameGenerator nameGenerator, ITimeNameMapper timeNameMapper, FunctionMapper functionMapper)
        {
            NameGenerator = nameGenerator ?? throw new ArgumentNullException(nameof(nameGenerator));
            TimeNameMapper = timeNameMapper ?? throw new ArgumentNullException(nameof(timeNameMapper));
            FunctionMapper = functionMapper ?? throw new ArgumentNullException(nameof(functionMapper));
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
                yield return new TimeExpandResult(TimeTypes.Second, timeField,name, timeField);
            }
            if ((type & TimeTypes.Minute) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Minute);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.MinuteFull("{0}");
                yield return new TimeExpandResult(TimeTypes.Minute, timeField, name, triggerExp);
            }
            if ((type & TimeTypes.Hour) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Hour);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.HourFull("{0}");
                yield return new TimeExpandResult(TimeTypes.Hour, timeField, name, triggerExp);
            }
            if ((type & TimeTypes.Day) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Day);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.DayFull("{0}");
                yield return new TimeExpandResult(TimeTypes.Day, timeField, name, triggerExp);
            }
            if ((type & TimeTypes.Week) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Week);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.WeekFull("{0}");
                yield return new TimeExpandResult(TimeTypes.Week, timeField, name, triggerExp);
            }
            if ((type & TimeTypes.Month) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Month);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.MonthFull("{0}");
                yield return new TimeExpandResult(TimeTypes.Month, timeField, name, triggerExp);
            }
            if ((type & TimeTypes.Quarter) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Quarter);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.QuarterFull("{0}");
                yield return new TimeExpandResult(TimeTypes.Quarter, timeField, name, triggerExp);
            }
            if ((type & TimeTypes.Year) != 0)
            {
                args[1] = TimeNameMapper.ToName(TimeTypes.Year);
                var timeField = NameGenerator.Create(args);
                var triggerExp = FunctionMapper.YearFull("{0}");
                yield return new TimeExpandResult(TimeTypes.Year, timeField, name, triggerExp);
            }
        }
    }
}
