using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe.Timescale
{
    public partial class TimescaleHelper
    {
        public string ApproximateRowCount(string relation)
        {
            return $"approximate_row_count({relation})";
        }
        public string First(string value,string time)
        {
            return $"first({value},{time})";
        }
        public string Last(string value, string time)
        {
            return $"last({value},{time})";
        }
        public string Last(string value, string min,string max,string nbuckets)
        {
            return $"histogram({value},{min},{max},{nbuckets})";
        }
        public string TimeBucket(string bucket_width, string ts,
            string? timezone = null,
            string? origin = null,
            string? offset = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(timezone))
                args.Add($"timezone => {timezone}");
            if (!string.IsNullOrEmpty(origin))
                args.Add($"origin => {origin}");
            if (!string.IsNullOrEmpty(offset))
                args.Add($"offset => {offset}");
            var sql = $"time_bucket({bucket_width},{ts}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string TimeBucketng(string bucket_width, string ts,
            string? origin = null,
            string? timezone = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(origin))
                args.Add($"origin => {origin}");
            if (!string.IsNullOrEmpty(timezone))
                args.Add($"timezone => {timezone}");
            var sql = $"timescaledb_experimental.time_bucket_ng({bucket_width},{ts}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string DaysInMonth(string date)
        {
            return $"days_in_month({date})";
        }
        public string DaysInMonth(string metric,
            string reference_date,
            string days)
        {
            return $"month_normalize({metric},{reference_date},{days})";
        }
        public string Hyperloglog(string buckets,
            string value)
        {
            return $"hyperloglog({buckets},{value})";
        }
        public string ApproxCountDistinct(string value)
        {
            return $"toolkit_experimental.approx_count_distinct({value})";
        }
        public string DistinctCount(string hyperloglog)
        {
            return $"distinct_count({hyperloglog})";
        }
        public string StdError(string hyperloglog)
        {
            return $"stderror({hyperloglog})";
        }
        public string Rollup(string hyperloglog)
        {
            return $"rollup({hyperloglog})";
        }
    }
}
