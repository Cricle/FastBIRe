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
        public string SaturatingAdd(string x, string y)
        {
            return $"saturating_add({x},{y})";
        }
        public string SaturatingAddPos(string x, string y)
        {
            return $"saturating_add_pos({x},{y})";
        }
        public string SaturatingMul(string x, string y)
        {
            return $"saturating_mul({x},{y})";
        }
        public string SaturatingSub(string x, string y)
        {
            return $"saturating_sub({x},{y})";
        }
        public string SaturatingSubPos(string x, string y)
        {
            return $"saturating_sub_pos({x},{y})";
        }
        public string StatsAgg(string value)
        {
            return $"stats_agg({value})";
        }
        public string Average(string summary)
        {
            return $"average({summary})";
        }
        public string Kurtosis(string summary,string? method=null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"kurtosis({summary}{methodStr})";
        }
        public string NumVals(string summary)
        {
            return $"num_vals({summary})";
        }
        public string Skewness(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"skewness({summary}{methodStr})";
        }
        public string Stddev(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"stddev({summary}{methodStr})";
        }
        public string Sum(string summary)
        {
            return $"sum({summary})";
        }
        public string Variance(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"variance({summary}{methodStr})";
        }
        public string Rolling(string ss)
        {
            return $"rolling({ss})";
        }
        public string StatsAgg(string x,string y)
        {
            return $"stats_agg({x},{y})";
        }
        public string AverageY(string summary)
        {
            return $"average_y({summary})";
        }
        public string AverageX(string summary)
        {
            return $"average_x({summary})";
        }
        public string Corr(string summary)
        {
            return $"corr({summary})";
        }
        public string Covariance(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"covariance({summary}{methodStr})";
        }
        public string DeterminationCoeff(string summary)
        {
            return $"determination_coeff({summary})";
        }
        public string Intercept(string summary)
        {
            return $"intercept({summary})";
        }
        public string KurtosisY(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"kurtosis_y({summary}{methodStr})";
        }
        public string KurtosisX(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"kurtosis_x({summary}{methodStr})";
        }
        public string SkewnessY(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"skewness_y({summary}{methodStr})";
        }
        public string SkewnessX(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"skewness_x({summary}{methodStr})";
        }
        public string Slope(string summary)
        {
            return $"slope({summary})";
        }
        public string StddevY(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"stddev_y({summary}{methodStr})";
        }
        public string StddevX(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"stddev_x({summary}{methodStr})";
        }
        public string SumY(string summary)
        {
            return $"sum_y({summary})";
        }
        public string SumX(string summary)
        {
            return $"sum_x({summary})";
        }
        public string VarianceY(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"variance_y({summary}{methodStr})";
        }
        public string VarianceX(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"variance_x({summary}{methodStr})";
        }
        public string XIntercept(string summary)
        {
            return $"x_intercept({summary})";
        }
        public string MinN(string value,string capacity)
        {
            return $"min_n({value},{capacity})";
        }
        public string IntoArray(string agg)
        {
            return $"into_array({agg})";
        }
        public string IntoValues(string agg)
        {
            return $"into_values({agg})";
        }
        public string IntoValues(string agg,string dummy)
        {
            return $"into_values({agg},{dummy})";
        }
        public string MaxN(string value, string capacity)
        {
            return $"max_n({value},{capacity})";
        }
        public string MinNBy(string value,string data, string capacity)
        {
            return $"min_n_by({value},{data},{capacity})";
        }
        public string MaxNBy(string value, string data, string capacity)
        {
            return $"max_n_by({value},{data},{capacity})";
        }
        public string CandlestickAgg(string ts, string price, string volume)
        {
            return $"candlestick_agg({ts},{price},{volume})";
        }
        public string Candlestick(string ts, 
            string open,
            string high,
            string low,
            string close,
            string volume)
        {
            return $"candlestick({ts},{open},{high},{low},{close},{volume})";
        }
        public string Close(string candlestick)
        {
            return $"close({candlestick})";
        }
        public string CloseTime(string candlestick)
        {
            return $"close_time({candlestick})";
        }
        public string High(string candlestick)
        {
            return $"high({candlestick})";
        }
        public string HighTime(string candlestick)
        {
            return $"high_time({candlestick})";
        }
        public string Low(string candlestick)
        {
            return $"low({candlestick})";
        }
        public string LowTime(string candlestick)
        {
            return $"low_time({candlestick})";
        }
        public string Open(string candlestick)
        {
            return $"open({candlestick})";
        }
        public string OpenTime(string candlestick)
        {
            return $"open_time({candlestick})";
        }
        public string Volume(string candlestick)
        {
            return $"volume({candlestick})";
        }
        public string Vwap(string candlestick)
        {
            return $"vwap({candlestick})";
        }
        public string TimeBucketGapfill(string bucket_width,
            string time,
            string? timezone = null,
            string? initial_start = null,
            string? start = null,
            string? finish = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(timezone))
                args.Add($"timezone => {timezone}");
            if (!string.IsNullOrEmpty(initial_start))
                args.Add($"initial_start => {initial_start}");
            if (!string.IsNullOrEmpty(start))
                args.Add($"start => {start}");
            if (!string.IsNullOrEmpty(finish))
                args.Add($"finish => {finish}");
            var sql = $"time_bucket_gapfill({bucket_width},{time}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string Interpolate(string value,
            string? prev = null,
            string? next = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(prev))
                args.Add($"prev => {prev}");
            if (!string.IsNullOrEmpty(next))
                args.Add($"next => {next}");
            var sql = $"interpolate({value}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string Locf(string value,
            string? prev = null,
            bool? treat_null_as_missing = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(prev))
                args.Add($"prev => {prev}");
            if (treat_null_as_missing != null) 
                args.Add($"treat_null_as_missing => {BoolToString(treat_null_as_missing)}");
            var sql = $"locf({value}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string Uddsketch(string size,
            string max_error,
            string value)
        {
            return $"uddsketch({size},{max_error},{value})";
        }
        public string PercentileAgg(string value)
        {
            return $"percentile_agg({value})";
        }
        public string ApproxPercentile(string percentile, string uddsketch)
        {
            return $"approx_percentile({percentile},{uddsketch})";
        }
        public string ApproxPercentileArray(string percentiles, string uddsketch)
        {
            return $"approx_percentile_array({percentiles},{uddsketch})";
        }
        public string ApproxPercentileRank(string value, string sketch)
        {
            return $"approx_percentile_rank({value},{sketch})";
        }
        public string Error(string sketch)
        {
            return $"error({sketch})";
        }
        public string Mean(string sketch)
        {
            return $"mean({sketch})";
        }
        public string Tdigest(string buckets, string value)
        {
            return $"tdigest({buckets},{value})";
        }
        public string CounterAgg(string ts,string value, string? bounds = null)
        {
            var boundsStr = bounds == null ? string.Empty : "," + bounds;

            return $"counter_agg({ts},{value}{boundsStr})";
        }
        public string CounterZeroTime(string summary)
        {
            return $"counter_zero_time({summary})";
        }
        public string Delta(string summary)
        {
            return $"delta({summary})";
        }
        public string ExtrapolatedDelta(string summary,string method)
        {
            return $"extrapolated_delta({summary},{method})";
        }
        public string ExtrapolatedRate(string summary, string method)
        {
            return $"extrapolated_rate({summary},{method})";
        }
        public string FirstTime(string cs)
        {
            return $"first_time({cs})";
        }
        public string FirstVal(string cs)
        {
            return $"first_val({cs})";
        }
        public string IdeltaLeft(string summary)
        {
            return $"idelta_left({summary})";
        }
        public string IdeltaRight(string summary)
        {
            return $"idelta_left({summary})";
        }
        public string InterpolatedDelta(string summary,
            string start,
            string interval,
            string? prev=null,
            string? next = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(prev))
                args.Add($"prev => {prev}");
            if (!string.IsNullOrEmpty(next))
                args.Add($"next => {next}");
            var sql = $"interpolated_delta({summary},{start},{interval}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string InterpolatedRate(string summary,
            string start,
            string interval,
            string? prev = null,
            string? next = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(prev))
                args.Add($"prev => {prev}");
            if (!string.IsNullOrEmpty(next))
                args.Add($"next => {next}");
            var sql = $"interpolated_rate({summary},{start},{interval}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string IrateLeft(string summary)
        {
            return $"irate_left({summary})";
        }
        public string IrateRight(string summary)
        {
            return $"irate_right({summary})";
        }
        public string LastTime(string cs)
        {
            return $"last_time({cs})";
        }
        public string LastVal(string cs)
        {
            return $"last_val({cs})";
        }
        public string NumChanges(string summary)
        {
            return $"num_changes({summary})";
        }
        public string NumElements(string summary)
        {
            return $"num_elements({summary})";
        }
        public string NumResets(string summary)
        {
            return $"num_resets({summary})";
        }
        public string Rate(string summary)
        {
            return $"rate({summary})";
        }
        public string TimeDelta(string summary)
        {
            return $"time_delta({summary})";
        }
        public string WithBounds(string summary,string bounds)
        {
            return $"with_bounds({summary},{bounds})";
        }
        public string GaugeAgg(string ts, string value, string? bounds = null)
        {
            var boundsStr = bounds == null ? string.Empty : "," + bounds;

            return $"gauge_agg({ts},{value}{boundsStr})";
        }
        public string TimeWeight(string method,string ts, string value)
        {
            return $"time_weight({method},{ts},{value})";
        }
        public string Integral(string tws, string? unit=null)
        {
            var unitStr = unit == null ? string.Empty : "," + unit;

            return $"integral({tws},{unitStr})";
        }
        public string InterpolatedIntegral(string tws,
            string start,
            string interval,
            string? prev = null,
            string? next = null,
            string? unit = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(prev))
                args.Add($"prev => {prev}");
            if (!string.IsNullOrEmpty(next))
                args.Add($"next => {next}");
            if (!string.IsNullOrEmpty(unit))
                args.Add($"unit => {unit}");

            var sql = $"interpolated_integral({tws},{start},{interval}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
    }
}
