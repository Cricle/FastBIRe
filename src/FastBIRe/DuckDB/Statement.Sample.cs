using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe.DuckDB
{
    public partial class Statement
    {
        public static string Sample(string value,SampleTypes type)
        {
            var unit = type == SampleTypes.Percent ? "%" : string.Empty;
            return $"USING SAMPLE {value}{unit}";
        }
        public static string BeforeJoinSample(string value,SampleTypes type,string otherTable)
        {
            var unit = type == SampleTypes.Percent ? "%" : string.Empty;
            return $"TABLESAMPLE RESERVOIR({value}{unit}), {otherTable}";
        }
        public static string AfterJoinSample(string value, SampleTypes type, string otherTable)
        {
            var unit = type == SampleTypes.Percent ? "%" : string.Empty;
            return $"USING SAMPLE RESERVOIR({value}{unit}), {otherTable}";
        }
    }
}
