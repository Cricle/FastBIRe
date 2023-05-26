using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe
{
    public partial class FunctionMapper
    {
        public string If(string condition,string @true, string @false)
        {
            return $@"CASE WHEN {condition} THEN {@true} ELSE {@false} END";
        }
        public string True()
        {
            return "true";
        }
        public string False()
        {
            return "true";
        }
        public string And(IEnumerable<string> inputs)
        {
            return string.Join(" AND ", inputs);
        }
        public string Or(IEnumerable<string> inputs)
        {
            return string.Join(" OR ", inputs);
        }
        public string Not(string input)
        {
            return "!" + input;
        }
    }
}
