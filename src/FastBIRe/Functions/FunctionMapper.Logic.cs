using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public partial class FunctionMapper
    {
        public string If(string condition, string @true, string @false)
        {
            return $@"CASE WHEN {condition} THEN {@true} ELSE {@false} END";
        }
        public string True()
        {
            if (SqlType == SqlType.SqlServer)
            {
                return "1";
            }
            return "true";
        }
        public string False()
        {
            if (SqlType == SqlType.SqlServer)
            {
                return "0";
            }
            return "false";
        }
        public string And(IEnumerable<string> inputs)
        {
            if (SqlType == SqlType.SqlServer)
            {
                return $"CASE WHEN {string.Join(" AND ", inputs.Select(CaseInput))} THEN 1 ELSE 0 END";
            }
            return string.Join(" AND ", inputs);
        }
        private string CaseInput(string input)
        {
            if (input == "0")
            {
                return "0=1";
            }
            else if (input == "1")
            {
                return "1=1";
            }
            return input;
        }
        public string Or(IEnumerable<string> inputs)
        {
            if (SqlType == SqlType.SqlServer)
            {
                return $"CASE WHEN {string.Join(" OR ", inputs.Select(CaseInput))} THEN 1 ELSE 0 END";
            }
            return string.Join(" OR ", inputs);
        }
        public string Not(string input)
        {
            if (SqlType == SqlType.SqlServer)
            {
                return $"CASE WHEN {input}=1 THEN 0 ELSE 1 END";
            }
            return "!" + input;
        }
    }
}
