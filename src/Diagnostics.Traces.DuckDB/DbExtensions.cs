using DuckDB.NET.Native;

namespace Diagnostics.Traces.DuckDB
{
    internal static class DbExtensions
    {
        public static int Execute(this DuckDBNativeConnection connection, string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return 0;
            }
            var results = PrepareMultiple(connection, sql);

            var count = 0;

            foreach (var result in results)
            {
                var current = result;
                count += (int)NativeMethods.Query.DuckDBRowsChanged(ref current);
            }

            return count;
        }
        private static IEnumerable<DuckDBResult> PrepareMultiple(DuckDBNativeConnection connection, string query)
        {
            using var unmanagedQuery = query.ToUnmanagedString();

            var statementCount = NativeMethods.ExtractStatements.DuckDBExtractStatements(connection, unmanagedQuery, out var extractedStatements);

            using (extractedStatements)
            {
                if (statementCount <= 0)
                {
                    var error = NativeMethods.ExtractStatements.DuckDBExtractStatementsError(extractedStatements);
                    throw new TraceDuckDbException(error.ToManagedString(false));
                }

                for (int index = 0; index < statementCount; index++)
                {
                    var status = NativeMethods.ExtractStatements.DuckDBPrepareExtractedStatement(connection, extractedStatements, index, out var statement);

                    if (status.IsSuccess())
                    {
                        using var result = Execute(statement);
                        yield return result;
                    }
                    else
                    {
                        var errorMessage = NativeMethods.PreparedStatements.DuckDBPrepareError(statement).ToManagedString(false);

                        throw new TraceDuckDbException(string.IsNullOrEmpty(errorMessage) ? "DuckDBQuery failed" : errorMessage, status);
                    }
                }
            }
        }
        public static DuckDBResult Execute(DuckDBPreparedStatement statement)
        {
            var status = NativeMethods.PreparedStatements.DuckDBExecutePreparedStreaming(statement, out var queryResult);
            if (!status.IsSuccess())
            {
                var errorMessage = NativeMethods.Query.DuckDBResultError(ref queryResult).ToManagedString(false);
                queryResult.Dispose();
                throw new TraceDuckDbException(string.IsNullOrEmpty(errorMessage) ? "DuckDBQuery failed" : errorMessage, status);
            }

            return queryResult;
        }
    }
}
