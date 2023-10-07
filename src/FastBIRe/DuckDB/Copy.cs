using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe.DuckDB
{
    public static class Copy
    {
        public static string CreateTableWithCsv(string tableName,string csvPath)
        {
            return $"CREATE TABLE \"{tableName}\" AS SELECT * FROM '{csvPath}';";
        }
        public static string ExportToCsv(string tableName, string csvPath)
        {
            return $"COPY (FROM \"{tableName}\") TO '{csvPath}' WITH (HEADER 1, DELIMITER '|');";
        }
    }
}
