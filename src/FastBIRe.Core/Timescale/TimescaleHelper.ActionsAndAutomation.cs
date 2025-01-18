namespace FastBIRe.Timescale
{
    public partial class TimescaleHelper
    {
        public string AddJob(string proc,
            string schedule_interval,
            string? config = null,
            string? initial_start = null,
            bool? scheduled = null,
            string? check_config = null,
            string? fixed_schedule = null,
            string? timezone = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(config))
                args.Add($"config => {config}");
            if (!string.IsNullOrEmpty(initial_start))
                args.Add($"initial_start => {initial_start}");
            if (scheduled != null)
                args.Add($"scheduled => {BoolToString(scheduled)}");
            if (!string.IsNullOrEmpty(check_config))
                args.Add($"check_config => {check_config}");
            if (!string.IsNullOrEmpty(fixed_schedule))
                args.Add($"fixed_schedule => {fixed_schedule}");
            if (!string.IsNullOrEmpty(timezone))
                args.Add($"timezone => {timezone}");
            var sql = $"add_job({proc},{schedule_interval}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string AlterJob(string job_id,
            string? schedule_interval = null,
            string? max_runtime = null,
            string? max_retries = null,
            string? retry_period = null,
            bool? scheduled = null,
            string? config = null,
            string? next_start = null,
            bool? if_exists = null,
            string? check_config = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(schedule_interval))
                args.Add($"schedule_interval => {schedule_interval}");
            if (!string.IsNullOrEmpty(max_runtime))
                args.Add($"max_runtime => {max_runtime}");
            if (!string.IsNullOrEmpty(max_retries))
                args.Add($"max_retries => {max_retries}");
            if (!string.IsNullOrEmpty(retry_period))
                args.Add($"retry_period => {retry_period}");
            if (scheduled != null)
                args.Add($"scheduled => {BoolToString(scheduled)}");
            if (!string.IsNullOrEmpty(config))
                args.Add($"config => {config}");
            if (!string.IsNullOrEmpty(next_start))
                args.Add($"next_start => {next_start}");
            if (if_exists != null)
                args.Add($"if_exists => {BoolToString(if_exists)}");
            if (!string.IsNullOrEmpty(check_config))
                args.Add($"check_config => {check_config}");
            var sql = $"alter_job({job_id}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string DeleteJob(string job_id)
        {
            return $"delete_job({job_id})";
        }
        public string RunJob(string job_id)
        {
            return $"run_job({job_id})";
        }
    }
}
