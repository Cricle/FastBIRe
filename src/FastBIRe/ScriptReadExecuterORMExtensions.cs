using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public static class ScriptReadExecuterORMExtensions
    {
        static class EmptyTaskResult<T>
        {
            public static readonly Task<T?> EmptyResult = Task.FromResult(default(T?));
        }
        public static Task<bool> ExistsAsync(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, static (o, e) =>
            {
                return Task.FromResult(e.Reader.Read());
            }, args: ParamterParser.Parse(args), transaction, token: token);
        }
        public static bool Exists(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null)
        {
            return scriptExecuter.ReadResult(script, static (o, e) => e.Reader.Read(), args: ParamterParser.Parse(args), transaction);
        }
        public static Task ClearTableAsync(this IDbScriptExecuter scriptExecuter, string tableName, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return scriptExecuter.ExecuteAsync($"DELETE FROM {scriptExecuter.SqlType.Wrap(tableName)}", transaction, token: token);
        }
        public static void ClearTable(this IDbScriptExecuter scriptExecuter, string tableName, DbTransaction? transaction = null)
        {
            scriptExecuter.ExecuteAsync($"DELETE FROM {scriptExecuter.SqlType.Wrap(tableName)}", transaction);
        }
        public static Task ReadTableAllColumnsAsync(this IDbScriptExecuter scriptExecuter, string tableName, ReadDataHandler handler, int? skip = null, int? take = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            var paggingPart = scriptExecuter.SqlType.GetTableHelper()!.Pagging(skip, take);
            var sql = $"SELECT * FROM {scriptExecuter.SqlType.Wrap(tableName)} {paggingPart}";
            return scriptExecuter.ReadAsync(sql, handler, transaction: transaction, token: token);
        }
        public static void ReadTableAllColumns(this IDbScriptExecuter scriptExecuter, string tableName, ReadDataHandlerSync handler, int? skip = null, int? take = null, DbTransaction? transaction = null)
        {
            var paggingPart = scriptExecuter.SqlType.GetTableHelper()!.Pagging(skip, take);
            var sql = $"SELECT * FROM {scriptExecuter.SqlType.Wrap(tableName)} {paggingPart}";
            scriptExecuter.Read(sql, handler, transaction);
        }
        public static Task ReadTableAllColumnsAsync(this IDbScriptExecuter scriptExecuter, string tableName, ReadDataHandlerSync handler, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadTableAllColumnsAsync(tableName, (o, e) =>
            {
                handler(o, e);
                return Task.CompletedTask;
            }, transaction: transaction, token: token);
        }
        public static Task<int> ExecuteAsync(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return scriptExecuter.ExecuteAsync(script, args: ParamterParser.Parse(args), transaction, token: token);
        }
        public static int Execute(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return scriptExecuter.Execute(script, args: ParamterParser.Parse(args), transaction, token: token);
        }
        public static Task ReadAsync(this IScriptExecuter scriptExecuter, string script, ReadDataHandlerSync handler, object? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadAsync(script, (o, e) =>
            {
                handler(o, e);
                return Task.CompletedTask;
            }, args: ParamterParser.Parse(args), transaction, token: token);
        }
        public static void Read(this IScriptExecuter scriptExecuter, string script, ReadDataHandlerSync handler, object? args = null, DbTransaction? transaction = null)
        {
            scriptExecuter.Read(script, (o, e) =>
            {
                handler(o, e);
            }, args: ParamterParser.Parse(args), transaction);
        }
        public static Task<T?> ReadOneAsync<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, static (o, e) =>
            {
                if (e.Reader.Read())
                {
                    var result = RecordToObjectManager<T>.RecordToObject.To(e.Reader);
                    return Task.FromResult(result);
                }
                return EmptyTaskResult<T>.EmptyResult;
            }, args: ParamterParser.Parse(args), transaction, token: token);
        }
        public static T? ReadOne<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null)
        {
            return scriptExecuter.ReadResult(script, static (o, e) =>
            {
                if (e.Reader.Read())
                {
                    var result = RecordToObjectManager<T>.RecordToObject.To(e.Reader);
                    return result;
                }
                return default;
            }, args: ParamterParser.Parse(args), transaction);
        }
        public static Task<IList<T?>> ReadRowsAsync<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, (o, e) =>
            {
                var res = RecordToObjectManager<T>.RecordToObject.ToList(e.Reader);
                return Task.FromResult(res);
            }, args: ParamterParser.Parse(args), transaction, token: token);
        }
        public static IList<T?> ReadRows<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null)
        {
            return scriptExecuter.ReadResult(script, static (o, e) => RecordToObjectManager<T>.RecordToObject.ToList(e.Reader), args: ParamterParser.Parse(args), transaction);
        }
        public static Task<IList<T?>> ReadRowsAsync<T>(this IScriptExecuter scriptExecuter, string script, Func<IDataReader, T?> reader, object? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, (o, e) =>
            {
                var res = new List<T?>();
                while (e.Reader.Read())
                {
                    res.Add(reader(e.Reader));
                }
                return Task.FromResult<IList<T?>>(res);
            }, args: ParamterParser.Parse(args), transaction, token: token);
        }
        public static IList<T?> ReadRows<T>(this IScriptExecuter scriptExecuter, string script, Func<IDataReader, T?> reader, object? args = null, DbTransaction? transaction = null)
        {
            return scriptExecuter.ReadResult(script, (o, e) =>
            {
                var res = new List<T?>();
                while (e.Reader.Read())
                {
                    res.Add(reader(e.Reader));
                }
                return res;
            }, args: ParamterParser.Parse(args), transaction);
        }

        public static Task<IList<T?>> ReadAsync<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, static (o, e) => Task.FromResult(RecordToObjectManager<T>.ToList(e.Reader)), args: ParamterParser.Parse(args), transaction, token: token);
        }
        public static IList<T?> Read<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null)
        {
            return scriptExecuter.ReadResult(script, static (o, e) => RecordToObjectManager<T>.ToList(e.Reader), args: ParamterParser.Parse(args), transaction);
        }
        public static async IAsyncEnumerable<T?> EnumerableAsync<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null, [EnumeratorCancellation] CancellationToken token = default)
        {
            using (var result = await scriptExecuter.ReadAsync(script, ParamterParser.Parse(args), transaction: transaction, token))
            {
                var reader = result.Args.Reader;
                while (reader.Read())
                {
                    yield return result.Read<T?>();
                }
            }
        }
        public static IEnumerable<T?> Enumerable<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null)
        {
            using (var result = scriptExecuter.Read(script, ParamterParser.Parse(args), transaction))
            {
                var reader = result.Args.Reader;
                while (reader.Read())
                {
                    yield return result.Read<T?>();
                }
            }
        }

        public static Task EnumerableAsync<T>(this IScriptExecuter scriptExecuter, string script, Action<T?> receiver, object? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, (o, e) =>
            {
                foreach (var item in RecordToObjectManager<T>.Enumerable(e.Reader))
                {
                    receiver(item);
                }
                return Task.FromResult(false);
            }, args: ParamterParser.Parse(args), transaction, token: token);
        }
        public static void Enumerable<T>(this IScriptExecuter scriptExecuter, string script, Action<T?> receiver, DbTransaction? transaction = null, object? args = null)
        {
            scriptExecuter.ReadResult(script, (o, e) =>
            {
                foreach (var item in RecordToObjectManager<T>.Enumerable(e.Reader))
                {
                    receiver(item);
                }
                return false;
            }, args: ParamterParser.Parse(args), transaction);
        }

        public static Task<int> ExecuteInterpolatedAsync(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p", DbTransaction? transaction = null, CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ExecuteAsync(result.Sql, result.Arguments, transaction, token);
        }
        public static int ExecuteInterpolated(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p", DbTransaction? transaction = null)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.Execute(result.Sql, result.Arguments, transaction);
        }
        public static Task ReadInterpolatedAsync(this IDbScriptExecuter executer, ReadDataHandler handler, FormattableString f, string argPrefx = "p", DbTransaction? transaction = null, CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ReadAsync(result.Sql, handler, result.Arguments, transaction: transaction, token);
        }
        public static void ReadInterpolated(this IDbScriptExecuter executer, ReadDataHandler handler, FormattableString f, string argPrefx = "p", DbTransaction? transaction = null)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            executer.ReadAsync(result.Sql, handler, result.Arguments, transaction);
        }
        public static Task ReadOneInterpolatedAsync<TResult>(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p", DbTransaction? transaction = null, CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ReadOneAsync<TResult>(result.Sql, result.Arguments, transaction, token);
        }
        public static void ReadOneInterpolated<TResult>(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p", DbTransaction? transaction = null)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            executer.ReadOneAsync<TResult>(result.Sql, result.Arguments, transaction);
        }
        public static Task ReadOneInterpolatedAsync<TResult>(this IDbScriptExecuter executer, ReadDataResultHandler<TResult> handler, FormattableString f, string argPrefx = "p", DbTransaction? transaction = null, CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ReadResultAsync(result.Sql, handler, result.Arguments, transaction, token);
        }
        public static void ReadOneInterpolated<TResult>(this IDbScriptExecuter executer, ReadDataResultHandler<TResult> handler, FormattableString f, string argPrefx = "p", DbTransaction? transaction = null)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            executer.ReadResultAsync(result.Sql, handler, result.Arguments, transaction);
        }
        public static Task ExistsInterpolatedAsync(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p", DbTransaction? transaction = null, CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ExistsAsync(result.Sql, result.Arguments, transaction, token);
        }
        public static void ExistsInterpolated(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p", DbTransaction? transaction = null)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            executer.ExistsAsync(result.Sql, result.Arguments, transaction);
        }
        public static Task ExistsInterpolatedAsync<TResult>(this IDbScriptExecuter executer, Func<IDataReader, TResult?> reader, FormattableString f, string argPrefx = "p", DbTransaction? transaction = null, CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ReadRowsAsync(result.Sql, reader, result.Arguments, transaction, token);
        }
        public static void ExistsInterpolated<TResult>(this IDbScriptExecuter executer, Func<IDataReader, TResult?> reader, FormattableString f, string argPrefx = "p", DbTransaction? transaction = null)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            executer.ReadRowsAsync(result.Sql, reader, result.Arguments, transaction);
        }

        public static IEnumerable<T> AsEnumerable<T>(this IDataReader reader)
        {
            return new DataReaderEnumerable<T>(reader);
        }
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IDataReader reader)
        {
            return new DataReaderAsyncEnumerable<T>(reader);
        }
        public static IEnumerable<T> AsEnumerable<T>(this IScriptReadResult result)
        {
            return new DataReaderEnumerable<T>(result.Args.Reader);
        }
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IScriptReadResult result)
        {
            return new DataReaderAsyncEnumerable<T>(result.Args.Reader);
        }
    }
}
