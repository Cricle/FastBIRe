namespace FastBIRe.Timescale
{
    public partial class TimescaleHelper
    {
        public string CreateDistributedHypertable(string relation,
            string time_column_name,
            string? partitioning_column = null,
            string? number_partitions = null,
            string? associated_schema_name = null,
            string? associated_table_prefix = null,
            string? chunk_time_interval = null,
            bool? create_default_indexes = null,
            bool? if_not_exists = null,
            string? partitioning_func = null,
            bool? migrate_data = null,
            bool? time_partitioning_func = null,
            bool? replication_factor = null,
            bool? data_nodes = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(partitioning_column))
                args.Add($"partitioning_column => {partitioning_column}");
            if (!string.IsNullOrEmpty(number_partitions))
                args.Add($"number_partitions => {number_partitions}");
            if (!string.IsNullOrEmpty(associated_schema_name))
                args.Add($"associated_schema_name => {associated_schema_name}");
            if (!string.IsNullOrEmpty(associated_table_prefix))
                args.Add($"associated_table_prefix => {associated_table_prefix}");
            if (!string.IsNullOrEmpty(chunk_time_interval))
                args.Add($"chunk_time_interval => {chunk_time_interval}");
            if (create_default_indexes != null)
                args.Add($"create_default_indexes => {BoolToString(create_default_indexes)}");
            if (if_not_exists != null)
                args.Add($"if_not_exists => {BoolToString(if_not_exists)}");
            if (!string.IsNullOrEmpty(partitioning_func))
                args.Add($"partitioning_func => {partitioning_func}");
            if (migrate_data != null)
                args.Add($"migrate_data => {BoolToString(migrate_data)}");
            if (time_partitioning_func != null)
                args.Add($"time_partitioning_func => {BoolToString(time_partitioning_func)}");
            if (replication_factor != null)
                args.Add($"replication_factor => {BoolToString(replication_factor)}");
            if (data_nodes != null)
                args.Add($"data_nodes => {BoolToString(data_nodes)}");
            var sql = $"create_distributed_hypertable({relation},{time_column_name}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string AddDataNode(string node_name,
            string host,
            string? database = null,
            string? port = null,
            bool? if_not_exists = null,
            string? bootstrap = null,
            string? password = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(database))
                args.Add($"database => {database}");
            if (!string.IsNullOrEmpty(port))
                args.Add($"port => {port}");
            if (if_not_exists!=null)
                args.Add($"if_not_exists => {BoolToString(if_not_exists)}");
            if (!string.IsNullOrEmpty(bootstrap))
                args.Add($"bootstrap => {bootstrap}");
            if (!string.IsNullOrEmpty(password))
                args.Add($"password => {password}");
            var sql = $"add_data_node({node_name},{host}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string AttachDataNode(string node_name,
            string hypertable,
            string? repartition = null,
            bool? if_not_attached = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(repartition))
                args.Add($"repartition => {repartition}");
            if (if_not_attached != null)
                args.Add($"if_not_attached => {BoolToString(if_not_attached)}");
            var sql = $"attach_data_node({node_name},{hypertable}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string AlterDataNode(string node_name,
            string? host=null,
            string? database = null,
            string? port = null,
            bool? available = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(host))
                args.Add($"host => {host}");
            if (!string.IsNullOrEmpty(database))
                args.Add($"database => {database}");
            if (!string.IsNullOrEmpty(port))
                args.Add($"port => {port}");
            if (available != null)
                args.Add($"available => {BoolToString(available)}");
            var sql = $"alter_data_node({node_name}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string DetachDataNode(string node_name,
            string? hypertable = null,
            bool? if_attached = null,
            bool? force = null,
            bool? repartition = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(hypertable))
                args.Add($"hypertable => {hypertable}");
            if (if_attached != null)
                args.Add($"if_attached => {BoolToString(if_attached)}");
            if (force != null)
                args.Add($"force => {BoolToString(force)}");
            if (repartition != null)
                args.Add($"repartition => {BoolToString(repartition)}");
            var sql = $"detach_data_node({node_name}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string DeleteDataNode(string node_name,
            bool? if_exists = null,
            bool? force = null,
            bool? repartition = null)
        {
            var args = new List<string>(0);
            if (if_exists != null)
                args.Add($"if_exists => {BoolToString(if_exists)}");
            if (force != null)
                args.Add($"force => {BoolToString(force)}");
            if (repartition != null)
                args.Add($"repartition => {BoolToString(repartition)}");
            var sql = $"delete_data_node({node_name}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string DistributedExec(string query,
            string? node_list = null,
            bool? transactional = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(node_list))
                args.Add($"node_list => {node_list}");
            if (transactional != null)
                args.Add($"transactional => {BoolToString(transactional)}");
            var sql = $"distributed_exec({query}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string SetNumberPartitions(string hypertable,
            string number_partitions,
            string? dimension_name = null)
        {
            var args = new List<string>(0);
            if (!string.IsNullOrEmpty(dimension_name))
                args.Add($"dimension_name => {dimension_name}");
            var sql = $"set_number_partitions({hypertable},{number_partitions}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string SetReplicationFactor(string hypertable,
            string replication_factor)
        {
            return $"set_replication_factor({hypertable},{replication_factor})";
        }
        public string CopyChunk(string chunk,
            string source_node,
            string destination_node)
        {
            return $"timescaledb_experimental.copy_chunk({chunk},{source_node},{destination_node})";
        }
        public string MoveChunk(string chunk,
            string source_node,
            string destination_node)
        {
            return $"timescaledb_experimental.move_chunk({chunk},{source_node},{destination_node})";
        }
        public string CleanupCopyChunkOperation(string operation_id)
        {
            return $"timescaledb_experimental.cleanup_copy_chunk_operation({operation_id})";
        }
        public string CreateDistributedRestorePoint(string name)
        {
            return $"create_distributed_restore_point({name})";
        }
    }
}
