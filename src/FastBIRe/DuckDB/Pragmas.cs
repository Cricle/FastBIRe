using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe.DuckDB
{
    public static class Pragmas
    {
        public static string ShowDatabases()
        {
            return "PRAGMA database_list";
        }
        public static string ShowTables()
        {
            return "PRAGMA show_tables";
        }
        public static string ShowTablesExpanded()
        {
            return "PRAGMA show_tables_expanded";
        }
        public static string ShowTableInfo(string table)
        {
            return $"PRAGMA table_info('{table}')";
        }
        public static string Show(string table)
        {
            return $"PRAGMA show('{table}')";
        }
        public static string MemoryLimit(string limit)
        {
            return $"PRAGMA memory_limit='{limit}'";
        }
        public static string Thread(string thread)
        {
            return $"PRAGMA thread={thread}";
        }
        public static string DataBaseSize()
        {
            return $"PRAGMA database_size";
        }
        public static string Collations()
        {
            return $"PRAGMA collations";
        }
        public static string DefaultCollation(string collation)
        {
            return $"PRAGMA default_collation='{collation}'";
        }
        public static string DefaultNullOrder(string nullOrder)
        {
            return $"PRAGMA default_null_order='{nullOrder}'";
        }
        public static string DefaultOrder(string order)
        {
            return $"PRAGMA default_order='{order}'";
        }
        public static string Version()
        {
            return $"PRAGMA version";
        }
        public static string Platform()
        {
            return $"PRAGMA platform";
        }
        public static string EnableProgressBar()
        {
            return $"PRAGMA enable_progress_bar";
        }
        public static string DisableProgressBar()
        {
            return $"PRAGMA disable_progress_bar";
        }
        public static string EnableProfiling()
        {
            return $"PRAGMA enable_profiling";
        }
        public static string DisableProfiling()
        {
            return $"PRAGMA disable_profiling";
        }
        public static string ProfilingOutput(string output)
        {
            return $"PRAGMA profiling_output='{output}'";
        }
        public static string ProfileOutput(string output)
        {
            return $"PRAGMA profile_output='{output}'";
        }
        public static string DisableOptimizer()
        {
            return $"PRAGMA disable_optimizer";
        }
        public static string EnableOptimizer()
        {
            return $"PRAGMA enable_optimizer";
        }
        public static string LogQueryPath(string path)
        {
            return $"PRAGMA log_query_path='{path}'";
        }
        public static string ExplainOutput(string output)
        {
            return $"PRAGMA explain_output='{output}'";
        }
        public static string EnableVerification()
        {
            return $"PRAGMA enable_verification";
        }
        public static string DisableVerification()
        {
            return $"PRAGMA disable_verification";
        }
        public static string VerifyParallelism()
        {
            return $"PRAGMA verify_parallelism";
        }
        public static string DisableVerifyParallelism()
        {
            return $"PRAGMA disable_verify_parallelism";
        }
        public static string ForceIndexJoin()
        {
            return $"PRAGMA force_index_join";
        }
        public static string VerifyExternal()
        {
            return $"PRAGMA verify_external";
        }
        public static string DisableVerifyExternal()
        {
            return $"PRAGMA disable_verify_external";
        }
        public static string VerifySerializer()
        {
            return $"PRAGMA verify_serializer";
        }
        public static string DisableVerifySerializer()
        {
            return $"PRAGMA disable_verify_serializer";
        }
        public static string EnableObjectCache()
        {
            return $"PRAGMA enable_object_cache";
        }
        public static string DisableObjectCache()
        {
            return $"PRAGMA disable_object_cache";
        }
        public static string ForceCheckpoint()
        {
            return $"PRAGMA force_checkpoint";
        }
        public static string EnablePrintProgressBar()
        {
            return $"PRAGMA enable_print_progress_bar";
        }
        public static string DisablePrintProgressBar()
        {
            return $"PRAGMA disable_print_progress_bar";
        }
        public static string EnableCheckpointOnShutdown()
        {
            return $"PRAGMA enable_checkpoint_on_shutdown";
        }
        public static string DisableCheckpointOnShutdown()
        {
            return $"PRAGMA disable_checkpoint_on_shutdown";
        }
        public static string TempDirectory(string path)
        {
            return $"PRAGMA temp_directory='{path}'";
        }
        public static string StorageInfo(string table)
        {
            return $"PRAGMA storage_info('{table}')";
        }
    }
}
