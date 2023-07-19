using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe.Timescale
{
    public partial class TimescaleHelper
    {
        public string AddRetentionPolicy(string relation,
            string drop_after,
            string initial_start,
            string timezone,
            bool? if_not_exists = null)
        {
            var args = new List<string>(0);
            if (if_not_exists != null)
                args.Add($"if_not_exists => {BoolToString(if_not_exists)}");
            var sql = $"add_retention_policy({relation},{drop_after},{initial_start},{timezone}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string RemoveRetentionPolicy(string relation)
        {
            return $"remove_retention_policy({relation})";
        }
    }
}
