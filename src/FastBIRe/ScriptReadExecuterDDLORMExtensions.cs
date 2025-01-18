using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace FastBIRe
{
    public static class ScriptReadExecuterDDLORMExtensions
    {
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
        public static Task<IDataSchemaDataTable> ReadTableAsync(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadTableAsync(script, ParamterParser.Parse(args), transaction, token);
        }
        public static async Task<IDataSchemaDataTable> ReadTableAsync(this IScriptExecuter scriptExecuter, string script, IEnumerable<KeyValuePair<string, object?>>? args = null, DbTransaction? transaction = null, CancellationToken token = default)
        {
            using (var scope = await scriptExecuter.ReadAsync(script, args, transaction, token))
            {
                return scope.Args.Reader.ToSchemaTable();
            }
        }
        public static IDataSchemaDataTable ReadTable(this IScriptExecuter scriptExecuter, string script, object? args = null, DbTransaction? transaction = null)
        {
            return scriptExecuter.ReadTable(script, ParamterParser.Parse(args), transaction);
        }
        public static IDataSchemaDataTable ReadTable(this IScriptExecuter scriptExecuter, string script, IEnumerable<KeyValuePair<string, object?>>? args = null, DbTransaction? transaction = null)
        {
            using (var scope = scriptExecuter.Read(script, args, transaction))
            {
                return scope.Args.Reader.ToSchemaTable();
            }
        }

    }
}
