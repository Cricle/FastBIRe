namespace FastBIRe.Timescale
{
    public partial class TimescaleHelper
    {
        public string CreateContinuousAggregate(string viewName,
            string query,
            bool? materialized_only=null,
            bool? create_group_indexes=null,
            bool? finalized=null)
        {
            var args = new List<string>();
            if (materialized_only != null)
                args.Add($"timescaledb.materialized_only = {BoolToString(materialized_only)}");
            if (create_group_indexes != null)
                args.Add($"timescaledb.create_group_indexes = {BoolToString(create_group_indexes)}");
            if (finalized != null)
                args.Add($"timescaledb.finalized = {BoolToString(finalized)}");
            var with = string.Empty;
            if (args.Count!=0)
            {
                with = "," + string.Join(",", args);
            }
            return $@"
CREATE MATERIALIZED VIEW {viewName}
WITH ({with}) AS 
{query}";
        }
        public string AlterContinuousAggregate(string viewName,
            bool? materialized_only = null,
            bool? create_group_indexes = null,
            bool? finalized = null)
        {
            var args = new List<string>();
            if (materialized_only != null)
                args.Add($"timescaledb.materialized_only = {BoolToString(materialized_only)}");
            if (create_group_indexes != null)
                args.Add($"timescaledb.create_group_indexes = {BoolToString(create_group_indexes)}");
            if (finalized != null)
                args.Add($"timescaledb.finalized = {BoolToString(finalized)}");
            if (args.Count==0)
            {
                return string.Empty;
            }
            return $@"ALTER MATERIALIZED VIEW {viewName} SET ({string.Join(",",args)})";
        }
        public string DropContinuousAggregate(string viewName)
        {
            return $@"DROP MATERIALIZED VIEW {viewName}";
        }
        public string RefreshContinuousAggregate(string continuous_aggregate,
            string window_start,
            string window_end)
        {
            return $@"refresh_continuous_aggregate({continuous_aggregate},{window_start},{window_end})";
        }
        public string AddContinuousAggregatePolicy(string continuous_aggregate,
            string start_offset,
            string end_offset,
            string schedule_interval,
            string initial_start,
            string timezone,
            bool? if_not_exists = null)
        {
            var args = new List<string>();
            if (if_not_exists != null)
                args.Add($"if_not_exists= {BoolToString(if_not_exists)}");
            if (args.Count == 0)
            {
                return string.Empty;
            }
            var sql = $"add_continuous_aggregate_policy({continuous_aggregate},{start_offset},{end_offset},{schedule_interval},{initial_start},{timezone}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string AddPolicies(string relation,
            bool? if_not_exists = null,
            string? refresh_start_offset = null,
            string? refresh_end_offset = null,
            string? compress_after = null,
            string? drop_after = null)
        {
            var args = new List<string>();
            if (if_not_exists != null)
                args.Add($"if_not_exists=> {BoolToString(if_not_exists)}");
            if (!string.IsNullOrEmpty(refresh_start_offset))
                args.Add($"refresh_start_offset=> {refresh_start_offset}");
            if (!string.IsNullOrEmpty(refresh_end_offset))
                args.Add($"refresh_end_offset => {refresh_end_offset}");
            if (!string.IsNullOrEmpty(compress_after))
                args.Add($"compress_after => {compress_after}");
            if (!string.IsNullOrEmpty(drop_after))
                args.Add($"drop_after => {drop_after}");
            if (args.Count == 0)
            {
                return string.Empty;
            }
            var sql = $"timescaledb_experimental.add_policies({relation}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string AlterPolicies(string relation,
            bool? if_not_exists = null,
            string? refresh_start_offset = null,
            string? refresh_end_offset = null,
            string? compress_after = null,
            string? drop_after = null)
        {
            var args = new List<string>();
            if (if_not_exists != null)
                args.Add($"if_not_exists=> {BoolToString(if_not_exists)}");
            if (!string.IsNullOrEmpty(refresh_start_offset))
                args.Add($"refresh_start_offset=> {refresh_start_offset}");
            if (!string.IsNullOrEmpty(refresh_end_offset))
                args.Add($"refresh_end_offset => {refresh_end_offset}");
            if (!string.IsNullOrEmpty(compress_after))
                args.Add($"compress_after => {compress_after}");
            if (!string.IsNullOrEmpty(drop_after))
                args.Add($"drop_after => {drop_after}");
            if (args.Count == 0)
            {
                return string.Empty;
            }
            var sql = $"timescaledb_experimental.alter_policies({relation}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string ShowPolicies(string relation)
        {
            return $@"timescaledb_experimental.show_policies({relation})";
        }
        public string RemoveContinuousAggregatePolicy(string continuous_aggregate,
            bool? if_exists = null)
        {
            var args = new List<string>();
            if (if_exists != null)
                args.Add($"if_exists = {BoolToString(if_exists)}");
            if (args.Count == 0)
            {
                return string.Empty;
            }
            var sql = $"remove_continuous_aggregate_policy({continuous_aggregate}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string CaggMigrate(string cagg,
            bool? @override = null,
            bool? drop_old=null)
        {
            var args = new List<string>();
            if (@override != null)
                args.Add($"override = {BoolToString(@override)}");
            if (drop_old != null)
                args.Add($"drop_old = {BoolToString(drop_old)}");
            if (args.Count == 0)
            {
                return string.Empty;
            }
            var sql = $"cagg_migrate({cagg}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string CaggMigrate(string relation,
            bool? if_exists = null,
            string? policy_names = null)
        {
            var args = new List<string>();
            if (if_exists != null)
                args.Add($"if_exists = {BoolToString(if_exists)}");
            if (!string.IsNullOrEmpty(policy_names))
                args.Add($"policy_names = {policy_names}");
            if (args.Count == 0)
            {
                return string.Empty;
            }
            var sql = $"timescaledb_experimental.remove_policies({relation}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string RemoveAllPolicies(string relation,
            bool? if_exists = null)
        {
            var args = new List<string>();
            if (if_exists != null)
                args.Add($"if_exists = {BoolToString(if_exists)}");
            if (args.Count == 0)
            {
                return string.Empty;
            }
            var sql = $"timescaledb_experimental.remove_all_policies({relation}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string HypertableSize(string hypertable)
        {
            return $"hypertable_size({hypertable})";
        }
        public string HypertableDetailedSize(string hypertable)
        {
            return $"hypertable_detailed_size({hypertable})";
        }
    }
}
