using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe.Timescale
{
    public partial class TimescaleHelper
    {
        public string ShowChunks(string tableName, string? older_than = null, string? newer_than = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(older_than))
                args.Add($"older_than => {older_than}");
            if (!string.IsNullOrEmpty(newer_than))
                args.Add($"newer_than => {newer_than}");
            var sql = $"show_chunks({tableName}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }


        public string DropChunks(string tableName,
            string? older_than = null,
            string? newer_than = null,
            bool? verbose = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(older_than))
                args.Add($"older_than => {older_than}");
            if (!string.IsNullOrEmpty(newer_than))
                args.Add($"newer_than => {newer_than}");
            if (verbose != null)
                args.Add($"verbose => {BoolToString(verbose)}");
            var sql = $"drop_chunks({tableName}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string ReorderChunks(string chunk,
            string? index = null,
            bool? verbose = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(index))
                args.Add($"index => {index}");
            if (verbose != null)
                args.Add($"verbose => {BoolToString(verbose)}");
            var sql = $"reorder_chunk({chunk}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string MoveChunks(string chunk,
            string destination_tablespace,
            string index_destination_tablespace,
            string? reorder_index = null,
            bool? verbose = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(reorder_index))
                args.Add($"reorder_index => {reorder_index}");
            if (verbose != null)
                args.Add($"verbose => {BoolToString(verbose)}");
            var sql = $"move_chunk({chunk},{destination_tablespace},{index_destination_tablespace}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string AddReorderPolicy(string hypertable,
            string index_name,
            string initial_start,
            string timezone,
            string? if_not_exists = null,
            bool? verbose = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(if_not_exists))
                args.Add($"if_not_exists => {if_not_exists}");
            if (verbose != null)
                args.Add($"verbose => {BoolToString(verbose)}");
            var sql = $"add_reorder_policy({hypertable},{index_name},{initial_start},{timezone}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string RemoveReorderPolicy(string hypertable,
            bool? if_exists = null)
        {
            var args = new List<string>(0);
            if (if_exists != null)
                args.Add($"if_exists => {BoolToString(if_exists)}");
            var sql = $"remove_reorder_policy({hypertable}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string AttachTablespace(string tablespace,
            string hypertable,
            bool? if_not_attached = null)
        {
            var args = new List<string>(0);
            if (if_not_attached != null)
                args.Add($"if_not_attached => {BoolToString(if_not_attached)}");
            var sql = $"remove_reorder_policy({tablespace},{hypertable}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string DetachTablespace(string tablespace,
            string? hypertable = null,
            bool? if_attached = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(hypertable))
                args.Add($"hypertable => {hypertable}");
            if (if_attached != null)
                args.Add($"if_attached => {BoolToString(if_attached)}");
            var sql = $"detach_tablespace({tablespace}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string DetachTablespaces(string tablespace)
        {
            return $"detach_tablespaces({tablespace})";
        }
        public string ShowTablespaces(string hypertable)
        {
            return $"show_tablespaces({hypertable})";
        }
        public string SetChunkTimeInterval(string hypertable,
            string chunk_time_interval)
        {
            return $"set_chunk_time_interval({hypertable},{chunk_time_interval})";
        }
        public string SetIntegerNowFunc(string main_table,
            string integer_now_func,
            bool? replace_if_exists = null)
        {
            var args = new List<string>(0);
            if (replace_if_exists != null)
                args.Add($"replace_if_exists => {BoolToString(replace_if_exists)}");
            var sql = $"set_integer_now_func({main_table},{integer_now_func}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string AddDimension(string hypertable,
            string column_name,
            string? number_partitions = null,
            string? chunk_time_interval = null,
            string? partitioning_func = null,
            bool? if_not_exists = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(number_partitions))
                args.Add($"number_partitions => {number_partitions}");
            if (!string.IsNullOrEmpty(chunk_time_interval))
                args.Add($"chunk_time_interval => {chunk_time_interval}");
            if (!string.IsNullOrEmpty(partitioning_func))
                args.Add($"partitioning_func => {partitioning_func}");
            if (if_not_exists != null)
                args.Add($"if_not_exists => {BoolToString(if_not_exists)}");
            var sql = $"add_dimension({hypertable},{column_name}";
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
        public string HypertableIndexSize(string hypertable)
        {
            return $"hypertable_index_size({hypertable})";
        }
        public string ChunksDetailedSize(string hypertable)
        {
            return $"chunks_detailed_size({hypertable})";
        }
        public string CreateHypertable(string tableName, string timeColumn,
            string? partitioning_column = null,
            string? number_partitions = null,
            string? chunk_time_interval = null,
            bool? create_default_indexes = null,
            bool? if_not_exists = null,
            string? partitioning_func = null,
            string? associated_schema_name = null,
            string? associated_table_prefix = null,
            string? migrate_data = null,
            string? time_partitioning_func = null,
            string? replication_factor = null,
            string? data_nodes = null,
            bool? distributed = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(partitioning_column))
                args.Add($"partitioning_column => {partitioning_column}");
            if (!string.IsNullOrEmpty(number_partitions))
                args.Add($"number_partitions => {number_partitions}");
            if (!string.IsNullOrEmpty(chunk_time_interval))
                args.Add($"chunk_time_interval => {chunk_time_interval}");
            if (create_default_indexes != null)
                args.Add($"create_default_indexes => {BoolToString(create_default_indexes)}");
            if (if_not_exists != null)
                args.Add($"if_not_exists => {BoolToString(if_not_exists)}");
            if (!string.IsNullOrEmpty(associated_schema_name))
                args.Add($"associated_schema_name => {associated_schema_name}");
            if (!string.IsNullOrEmpty(associated_table_prefix))
                args.Add($"associated_table_prefix => {associated_table_prefix}");
            if (!string.IsNullOrEmpty(partitioning_func))
                args.Add($"partitioning_func => {partitioning_func}");
            if (!string.IsNullOrEmpty(migrate_data))
                args.Add($"migrate_data => {migrate_data}");
            if (!string.IsNullOrEmpty(time_partitioning_func))
                args.Add($"time_partitioning_func => {time_partitioning_func}");
            if (!string.IsNullOrEmpty(replication_factor))
                args.Add($"replication_factor => {replication_factor}");
            if (!string.IsNullOrEmpty(data_nodes))
                args.Add($"data_nodes => {data_nodes}");
            if (distributed != null)
                args.Add($"distributed => {BoolToString(distributed)}");
            var baseCall = $"create_hypertable({tableName},{timeColumn}";
            if (args.Count != 0)
            {
                baseCall += "," + string.Join(",", args);
            }
            return baseCall + ")";
        }
    }
}
