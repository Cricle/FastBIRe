namespace FastBIRe.Timescale
{
    public partial class TimescaleHelper
    {
        public static string ApproximateRowCount(string relation)
        {
            return $"approximate_row_count({relation})";
        }
        public static string First(string value, string time)
        {
            return $"first({value},{time})";
        }
        public static string Last(string value, string time)
        {
            return $"last({value},{time})";
        }
        public static string Last(string value, string min, string max, string nbuckets)
        {
            return $"histogram({value},{min},{max},{nbuckets})";
        }
        public static string TimeBucket(string bucket_width, string ts,
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
        public static string TimeBucketng(string bucket_width, string ts,
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
        public static string DaysInMonth(string date)
        {
            return $"days_in_month({date})";
        }
        public static string DaysInMonth(string metric,
            string reference_date,
            string days)
        {
            return $"month_normalize({metric},{reference_date},{days})";
        }
        public static string Hyperloglog(string buckets,
            string value)
        {
            return $"hyperloglog({buckets},{value})";
        }
        public static string ApproxCountDistinct(string value)
        {
            return $"toolkit_experimental.approx_count_distinct({value})";
        }
        public static string DistinctCount(string hyperloglog)
        {
            return $"distinct_count({hyperloglog})";
        }
        public static string StdError(string hyperloglog)
        {
            return $"stderror({hyperloglog})";
        }
        public static string Rollup(string hyperloglog)
        {
            return $"rollup({hyperloglog})";
        }
        public static string SaturatingAdd(string x, string y)
        {
            return $"saturating_add({x},{y})";
        }
        public static string SaturatingAddPos(string x, string y)
        {
            return $"saturating_add_pos({x},{y})";
        }
        public static string SaturatingMul(string x, string y)
        {
            return $"saturating_mul({x},{y})";
        }
        public static string SaturatingSub(string x, string y)
        {
            return $"saturating_sub({x},{y})";
        }
        public static string SaturatingSubPos(string x, string y)
        {
            return $"saturating_sub_pos({x},{y})";
        }
        public static string StatsAgg(string value)
        {
            return $"stats_agg({value})";
        }
        public static string Average(string summary)
        {
            return $"average({summary})";
        }
        public static string Kurtosis(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"kurtosis({summary}{methodStr})";
        }
        public static string NumVals(string summary)
        {
            return $"num_vals({summary})";
        }
        public static string Skewness(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"skewness({summary}{methodStr})";
        }
        public static string Stddev(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"stddev({summary}{methodStr})";
        }
        public static string Sum(string summary)
        {
            return $"sum({summary})";
        }
        public static string Variance(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"variance({summary}{methodStr})";
        }
        public static string Rolling(string ss)
        {
            return $"rolling({ss})";
        }
        public static string StatsAgg(string x, string y)
        {
            return $"stats_agg({x},{y})";
        }
        public static string AverageY(string summary)
        {
            return $"average_y({summary})";
        }
        public static string AverageX(string summary)
        {
            return $"average_x({summary})";
        }
        public static string Corr(string summary)
        {
            return $"corr({summary})";
        }
        public static string Covariance(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"covariance({summary}{methodStr})";
        }
        public static string DeterminationCoeff(string summary)
        {
            return $"determination_coeff({summary})";
        }
        public static string Intercept(string summary)
        {
            return $"intercept({summary})";
        }
        public static string KurtosisY(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"kurtosis_y({summary}{methodStr})";
        }
        public static string KurtosisX(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"kurtosis_x({summary}{methodStr})";
        }
        public static string SkewnessY(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"skewness_y({summary}{methodStr})";
        }
        public static string SkewnessX(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"skewness_x({summary}{methodStr})";
        }
        public static string Slope(string summary)
        {
            return $"slope({summary})";
        }
        public static string StddevY(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"stddev_y({summary}{methodStr})";
        }
        public static string StddevX(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"stddev_x({summary}{methodStr})";
        }
        public static string SumY(string summary)
        {
            return $"sum_y({summary})";
        }
        public static string SumX(string summary)
        {
            return $"sum_x({summary})";
        }
        public static string VarianceY(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"variance_y({summary}{methodStr})";
        }
        public static string VarianceX(string summary, string? method = null)
        {
            var methodStr = method == null ? string.Empty : "," + method;

            return $"variance_x({summary}{methodStr})";
        }
        public static string XIntercept(string summary)
        {
            return $"x_intercept({summary})";
        }
        public static string MinN(string value, string capacity)
        {
            return $"min_n({value},{capacity})";
        }
        public static string IntoArray(string agg)
        {
            return $"into_array({agg})";
        }
        public static string IntoValues(string agg)
        {
            return $"into_values({agg})";
        }
        public static string IntoValues(string agg, string dummy)
        {
            return $"into_values({agg},{dummy})";
        }
        public static string MaxN(string value, string capacity)
        {
            return $"max_n({value},{capacity})";
        }
        public static string MinNBy(string value, string data, string capacity)
        {
            return $"min_n_by({value},{data},{capacity})";
        }
        public static string MaxNBy(string value, string data, string capacity)
        {
            return $"max_n_by({value},{data},{capacity})";
        }
        public static string CandlestickAgg(string ts, string price, string volume)
        {
            return $"candlestick_agg({ts},{price},{volume})";
        }
        public static string Candlestick(string ts,
            string open,
            string high,
            string low,
            string close,
            string volume)
        {
            return $"candlestick({ts},{open},{high},{low},{close},{volume})";
        }
        public static string Close(string candlestick)
        {
            return $"close({candlestick})";
        }
        public static string CloseTime(string candlestick)
        {
            return $"close_time({candlestick})";
        }
        public static string High(string candlestick)
        {
            return $"high({candlestick})";
        }
        public static string HighTime(string candlestick)
        {
            return $"high_time({candlestick})";
        }
        public static string Low(string candlestick)
        {
            return $"low({candlestick})";
        }
        public static string LowTime(string candlestick)
        {
            return $"low_time({candlestick})";
        }
        public static string Open(string candlestick)
        {
            return $"open({candlestick})";
        }
        public static string OpenTime(string candlestick)
        {
            return $"open_time({candlestick})";
        }
        public static string Volume(string candlestick)
        {
            return $"volume({candlestick})";
        }
        public static string Vwap(string candlestick)
        {
            return $"vwap({candlestick})";
        }
        public static string TimeBucketGapfill(string bucket_width,
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
        public static string Interpolate(string value,
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
        public static string Locf(string value,
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
        public static string Uddsketch(string size,
            string max_error,
            string value)
        {
            return $"uddsketch({size},{max_error},{value})";
        }
        public static string PercentileAgg(string value)
        {
            return $"percentile_agg({value})";
        }
        public static string ApproxPercentile(string percentile, string uddsketch)
        {
            return $"approx_percentile({percentile},{uddsketch})";
        }
        public static string ApproxPercentileArray(string percentiles, string uddsketch)
        {
            return $"approx_percentile_array({percentiles},{uddsketch})";
        }
        public static string ApproxPercentileRank(string value, string sketch)
        {
            return $"approx_percentile_rank({value},{sketch})";
        }
        public static string Error(string sketch)
        {
            return $"error({sketch})";
        }
        public static string Mean(string sketch)
        {
            return $"mean({sketch})";
        }
        public static string Tdigest(string buckets, string value)
        {
            return $"tdigest({buckets},{value})";
        }
        public static string CounterAgg(string ts, string value, string? bounds = null)
        {
            var boundsStr = bounds == null ? string.Empty : "," + bounds;

            return $"counter_agg({ts},{value}{boundsStr})";
        }
        public static string CounterZeroTime(string summary)
        {
            return $"counter_zero_time({summary})";
        }
        public static string Delta(string summary)
        {
            return $"delta({summary})";
        }
        public static string ExtrapolatedDelta(string summary, string method)
        {
            return $"extrapolated_delta({summary},{method})";
        }
        public static string ExtrapolatedRate(string summary, string method)
        {
            return $"extrapolated_rate({summary},{method})";
        }
        public static string FirstTime(string cs)
        {
            return $"first_time({cs})";
        }
        public static string FirstVal(string cs)
        {
            return $"first_val({cs})";
        }
        public static string IdeltaLeft(string summary)
        {
            return $"idelta_left({summary})";
        }
        public static string IdeltaRight(string summary)
        {
            return $"idelta_left({summary})";
        }
        public static string InterpolatedDelta(string summary,
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
            var sql = $"interpolated_delta({summary},{start},{interval}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public static string InterpolatedRate(string summary,
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
        public static string IrateLeft(string summary)
        {
            return $"irate_left({summary})";
        }
        public static string IrateRight(string summary)
        {
            return $"irate_right({summary})";
        }
        public static string LastTime(string cs)
        {
            return $"last_time({cs})";
        }
        public static string LastVal(string cs)
        {
            return $"last_val({cs})";
        }
        public static string NumChanges(string summary)
        {
            return $"num_changes({summary})";
        }
        public static string NumElements(string summary)
        {
            return $"num_elements({summary})";
        }
        public static string NumResets(string summary)
        {
            return $"num_resets({summary})";
        }
        public static string Rate(string summary)
        {
            return $"rate({summary})";
        }
        public static string TimeDelta(string summary)
        {
            return $"time_delta({summary})";
        }
        public static string WithBounds(string summary, string bounds)
        {
            return $"with_bounds({summary},{bounds})";
        }
        public static string GaugeAgg(string ts, string value, string? bounds = null)
        {
            var boundsStr = bounds == null ? string.Empty : "," + bounds;

            return $"gauge_agg({ts},{value}{boundsStr})";
        }
        public static string TimeWeight(string method, string ts, string value)
        {
            return $"time_weight({method},{ts},{value})";
        }
        public static string Integral(string tws, string? unit = null)
        {
            var unitStr = unit == null ? string.Empty : "," + unit;

            return $"integral({tws},{unitStr})";
        }
        public static string InterpolatedIntegral(string tws,
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
