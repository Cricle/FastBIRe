using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public static class DateTimePartNamesExtensions
    {
        public static IEnumerable<string> WriteDatePartTrigger(this IDateTimePartNames timePartNames,string name,string table, string idColumn,string field,string type,string rawFormat,SqlType sqlType,ComputeTriggerHelper computeTriggerHelper)
        {
            var triggers = new List<TriggerField>();
            var prefx = sqlType == SqlType.SqlServer ? string.Empty : "NEW.";
            var useField = prefx + sqlType.Wrap(field);
            var helper = new MergeHelper(sqlType);
            foreach (var part in timePartNames.GetDatePartNames(field))
            {
                triggers.Add(new TriggerField(part.Key,
                    helper.ToRaw(part.Value, useField, false)!, field, type,rawFormat));
            }
            return computeTriggerHelper.Create(name, table, triggers, idColumn, sqlType);
        }
    }
    public class DateTimePartNames : IDateTimePartNames
    {
        public const string DefaultSystemPrefx = "__$";
        public const string DefaultSystemSuffix = "";

        public const string DefaultYear = "_year";
        public const string DefaultMonth = "_month";
        public const string DefaultDay = "_day";
        public const string DefaultHour = "_hour";
        public const string DefaultMinute = "_minute";
        public const string DefaultQuarter = "_quarter";
        public const string DefaultWeek = "_week";

        public static readonly DateTimePartNames Default = new DateTimePartNames(DefaultSystemPrefx,
            DefaultSystemSuffix,
            DefaultYear,
            DefaultMonth,
            DefaultDay,
            DefaultHour,
            DefaultMinute,
            DefaultQuarter,
            DefaultWeek);

        public DateTimePartNames(string systemPrefx, string systemSuffix, string year, string month, string day, string hour, string minute, string quarter, string week)
        {
            SystemPrefx = systemPrefx;
            SystemSuffix = systemSuffix;
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Quarter = quarter;
            Week = week;
        }

        public string SystemPrefx { get; }

        public string SystemSuffix { get; }

        public string Year { get; }

        public string Month { get; }

        public string Day { get; }

        public string Hour { get; }

        public string Minute { get; }

        public string Quarter { get; }

        public string Week { get; }

        public string CombineField(string field, string part)
        {
            return SystemPrefx + field + part + SystemSuffix;
        }
        public IReadOnlyList<KeyValuePair<string, ToRawMethod>> GetDatePartNames(string field)
        {
            return new KeyValuePair<string, ToRawMethod>[]
            {
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Year), ToRawMethod.Year),
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Month), ToRawMethod.Month),
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Day), ToRawMethod.Day),
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Hour), ToRawMethod.Hour),
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Minute), ToRawMethod.Minute),
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Quarter), ToRawMethod.Quarter),
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Week), ToRawMethod.Week),
            };
        }
        public bool TryGetField(ToRawMethod method, string field, out string? combinedField)
        {
            switch (method)
            {
                case ToRawMethod.Year:
                    combinedField = CombineField(field, Year);
                    return true;
                case ToRawMethod.Month:
                    combinedField = CombineField(field, Month);
                    return true;
                case ToRawMethod.Day:
                    combinedField = CombineField(field, Day);
                    return true;
                case ToRawMethod.Hour:
                    combinedField = CombineField(field, Hour);
                    return true;
                case ToRawMethod.Minute:
                    combinedField = CombineField(field, Minute);
                    return true;
                case ToRawMethod.Quarter:
                    combinedField = CombineField(field, Quarter);
                    return true;
                case ToRawMethod.Week:
                    combinedField = CombineField(field, Week);
                    return true;
                default:
                    combinedField = null;
                    return false;
            }
        }
    }
    public record SourceTableColumnDefine : TableColumnDefine
    {
        public SourceTableColumnDefine(string? field, string raw, bool isGroup, TableColumnDefine destColumn, ToRawMethod method, string rawFormat, bool onlySet = false)
            : base(field, raw, rawFormat, onlySet)
        {
            Field = field;
            Raw = raw;
            IsGroup = isGroup;
            DestColumn = destColumn;
            Method = method;
            RawFormat = rawFormat;
            OnlySet = onlySet;
        }

        public bool IsGroup { get; set; }

        public TableColumnDefine DestColumn { get; set; }

        public ToRawMethod Method { get; set; }

        public SourceTableColumnDefine Copy()
        {
            return new SourceTableColumnDefine(this);
        }
        public new SourceTableColumnDefine SetIndex(string group, int order = 0, bool desc = false)
        {
            base.SetIndex(group, order, desc);
            return this;
        }

        public SourceTableColumnDefine SetExpandDateTime(bool expand = true, bool destExpand = false)
        {
            base.SetExpandDateTime(expand);
            if (destExpand && DestColumn != null)
            {
                DestColumn.SetExpandDateTime(destExpand);
            }
            return this;
        }
        public new SourceTableColumnDefine SetExpandDateTime(bool expand = true)
        {
            base.SetExpandDateTime(expand);
            return this;
        }
        public SourceTableColumnDefine AllNotNull()
        {
            Nullable = false;
            if (DestColumn != null)
            {
                DestColumn.Nullable = false;
            }
            return this;
        }
    }
}
