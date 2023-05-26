using Ao.Stock.Querying;
using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public partial class FunctionMapper
    {
        public FunctionMapper(SqlType sqlType)
        {
            SqlType = sqlType;
            MethodWrapper = MergeHelper.GetMethodWrapper(sqlType);
        }

        public IMethodWrapper MethodWrapper { get; }

        public SqlType SqlType { get; }

        public string? WrapValue<T>(T value)
        {
            return MethodWrapper.WrapValue(value);
        }
        public string Quto(string name)
        {
            return MethodWrapper.Quto(name);
        }
        public string Bracket(string input)
        {
            return $"({input})";
        }
        public string? Map(SQLFunctions func, params string[] args)
        {
            switch (func)
            {
                case SQLFunctions.Date:
                    return Date(args[0], args[1], args[2]);
                case SQLFunctions.DateDif:
                    return DateDif(args[0], args[1], args[2]);
                case SQLFunctions.Day:
                    return Day(args[0]);
                case SQLFunctions.Year:
                    return Year(args[0]);
                case SQLFunctions.Month:
                    return Month(args[0]);
                case SQLFunctions.Minute:
                    return Minute(args[0]);
                case SQLFunctions.Second:
                    return Second(args[0]);
                case SQLFunctions.NetWorkDays:
                    return NetWorkDays(args[0], args[1],args.Skip(2));
                case SQLFunctions.Now:
                    return Now();
                case SQLFunctions.ToDay:
                    return ToDay();
                case SQLFunctions.Days:
                    return Days(args[0], args[1]);
                case SQLFunctions.Weakday:
                    return Weakday(args[0], args[1]);
                case SQLFunctions.WeakNum:
                    return WeakNum(args[0]);
                case SQLFunctions.WorkDay:
                    return WorkDay(args[0], args[1], args.Skip(2));
                case SQLFunctions.Abs:
                    return Abs(args[0]);
                case SQLFunctions.Rand:
                    return Rand();
                case SQLFunctions.RandBetween:
                    return RandBetween(args[0], args[1]);
                case SQLFunctions.Round:
                    return Round(args[0], args[1]);
                case SQLFunctions.RoundUp:
                    return RoundUp(args[0], args[1]);
                case SQLFunctions.RoundDown:
                    return RoundDown(args[0], args[1]);
                case SQLFunctions.Sum:
                    return Sum(args);
                case SQLFunctions.Count:
                    return Count(args);
                case SQLFunctions.CountA:
                    return CountA(args);
                case SQLFunctions.Average:
                    return Average(args);
                case SQLFunctions.Max:
                    return Max(args);
                case SQLFunctions.Min:
                    return Min(args);
                case SQLFunctions.Median:
                    return Median(args);
                case SQLFunctions.Char:
                    return Char(args[1]);
                case SQLFunctions.Concatenate:
                    return Concatenate(args);
                case SQLFunctions.Left:
                    return Left(args[0], args[1]);
                case SQLFunctions.Right:
                    return Right(args[0], args[1]);
                case SQLFunctions.Len:
                    return Len(args[0]);
                case SQLFunctions.Lower:
                    return Lower(args[0]);
                case SQLFunctions.Mid:
                    return Mid(args[0], args[1], args[2]);
                case SQLFunctions.Replace:
                    return Replace(args[0], args[1], args[2], args[3]);
                case SQLFunctions.ToDate:
                    return ToDate(args[0]);
                case SQLFunctions.Trim:
                    return Trim(args[0]);
                case SQLFunctions.Upper:
                    return Upper(args[0]);
                case SQLFunctions.If:
                    return If(args[0], args[1], args[2]);
                case SQLFunctions.True:
                    return True();
                case SQLFunctions.False:
                    return False();
                case SQLFunctions.And:
                    return And(args);
                case SQLFunctions.Or:
                    return Or(args);
                case SQLFunctions.Not:
                    return Not(args[0]);
                case SQLFunctions.Bracket:
                    return Bracket(args[0]);
                default:
                    return null;
            }
        }
    }
}
