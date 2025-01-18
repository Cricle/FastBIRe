namespace FastBIRe.Timescale
{
    public partial class TimescaleHelper
    {
        public string AddCompressionPolicy(string hypertable,
            string compress_after,
            string schedule_interval,
            string initial_start,
            string timezone)
        {
            return $"add_compression_policy({hypertable},{compress_after},{schedule_interval},{initial_start},{timezone})";
        }
        public string RemoveCompressionPolicy(string hypertable,
            bool? if_exists = null)
        {
            var args = new List<string>(0);
            if (if_exists != null)
                args.Add($"if_exists => {BoolToString(if_exists)}");
            var sql = $"remove_compression_policy({hypertable}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string CompressChunk(string hypertable,
            bool? if_not_compressed = null)
        {
            var args = new List<string>(0);
            if (if_not_compressed != null)
                args.Add($"if_not_compressed => {BoolToString(if_not_compressed)}");
            var sql = $"compress_chunk({hypertable}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string DecompressChunk(string chunk_name,
            bool? if_compressed = null)
        {
            var args = new List<string>(0);
            if (if_compressed != null)
                args.Add($"if_compressed => {BoolToString(if_compressed)}");
            var sql = $"decompress_chunk({chunk_name}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string RecompressChunk(string chunk,
            bool? if_not_compressed = null)
        {
            var args = new List<string>(0);
            if (if_not_compressed != null)
                args.Add($"if_not_compressed => {BoolToString(if_not_compressed)}");
            var sql = $"recompress_chunk({chunk}";
            if (args.Count != 0)
            {
                sql += "," + string.Join(",", args);
            }
            return sql + ")";
        }
        public string HypertableCompressionStats(string hypertable)
        {
            return $"hypertable_compression_stats({hypertable})";
        }
        public string ChunkCompressionStats(string hypertable)
        {
            return $"chunk_compression_stats({hypertable})";
        }
    }
}
