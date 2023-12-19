using DatabaseSchemaReader.DataSchema;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public static class ScriptReadExecuterORMExtensions
    {
        static class EmptyTaskResult<T>
        {
            public static readonly Task<T?> EmptyResult = Task.FromResult(default(T?));
        }
        class ObjectPropertyMap
        {
            public ObjectPropertyMap(string propertyName, Func<object, object?> propertyReader)
            {
                PropertyName = propertyName;
                PropertyReader = propertyReader;
            }

            public string PropertyName { get; }

            public Func<object, object?> PropertyReader { get; }
        }
        class ObjectVisitor
        {
            private static readonly Type ObjectType = typeof(object);

            public readonly Type Type;

            public readonly IList<ObjectPropertyMap> propertyReaders;

            public IEnumerable<KeyValuePair<string, object?>> EnumerableProperties(object instance)
            {
                for (int i = 0; i < propertyReaders.Count; i++)
                {
                    var map = propertyReaders[i];
                    yield return new KeyValuePair<string, object?>(map.PropertyName, map.PropertyReader(instance));
                }
            }

            public ObjectVisitor(Type type)
            {
                Type = type;
                propertyReaders = new ObjectPropertyMap[type.GetProperties().Length];
                Analysis();
            }

            public void Analysis()
            {
                var props = Type.GetProperties();
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    var par0 = Expression.Parameter(ObjectType);
                    var body = Expression.Convert(Expression.Call(Expression.Convert(par0, Type), prop.GetMethod!), ObjectType);
                    propertyReaders[i] = new ObjectPropertyMap(prop.Name, Expression.Lambda<Func<object, object?>>(body, par0).Compile());
                }
            }
        }
        private static readonly ConcurrentDictionary<Type, ObjectVisitor> objectVisitor = new ConcurrentDictionary<Type, ObjectVisitor>();
        private static IEnumerable<KeyValuePair<string, object?>>? PrepareArgs(object? obj)
        {
            if (obj == null)
            {
                return null;
            }
            if (obj is IEnumerable<KeyValuePair<string, object?>> args)
            {
                return args;
            }
            if (obj is IList list)
            {
                return EnumerableList(list);
            }
            var type = obj.GetType();
            if (type.IsClass)
            {
                return objectVisitor.GetOrAdd(type, static t => new ObjectVisitor(t)).EnumerableProperties(obj);
            }
            return new OneEnumerable<KeyValuePair<string, object?>>(new KeyValuePair<string, object?>(string.Empty, obj));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<KeyValuePair<string, object?>>? EnumerableList(IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                yield return new KeyValuePair<string, object?>(i.ToString(), list[i]);
            }
        }
        public static Task<bool> ExistsAsync(this IScriptExecuter scriptExecuter, string script, object? args = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, static (o, e) =>
            {
                return Task.FromResult(e.Reader.Read());
            }, args: PrepareArgs(args), token: token);
        }
        public static bool Exists(this IScriptExecuter scriptExecuter, string script, object? args = null)
        {
            return scriptExecuter.ReadResult(script, static (o, e) => e.Reader.Read(), args: PrepareArgs(args));
        }
        public static Task ClearTableAsync(this IDbScriptExecuter scriptExecuter,string tableName,CancellationToken token = default)
        {
            return scriptExecuter.ExecuteAsync($"DELETE FROM {scriptExecuter.SqlType.Wrap(tableName)}",token:token);
        }
        public static void ClearTable(this IDbScriptExecuter scriptExecuter, string tableName)
        {
            scriptExecuter.ExecuteAsync($"DELETE FROM {scriptExecuter.SqlType.Wrap(tableName)}");
        }
        public static Task ReadTableAllColumnsAsync(this IDbScriptExecuter scriptExecuter, string tableName, ReadDataHandler handler, int? skip = null, int? take = null, CancellationToken token = default)
        {
            var paggingPart = scriptExecuter.SqlType.GetTableHelper()!.Pagging(skip, take);
            var sql = $"SELECT * FROM {scriptExecuter.SqlType.Wrap(tableName)} {paggingPart}";
            return scriptExecuter.ReadAsync(sql, handler, token: token);
        }
        public static void ReadTableAllColumns(this IDbScriptExecuter scriptExecuter, string tableName, ReadDataHandlerSync handler, int? skip = null, int? take = null)
        {
            var paggingPart = scriptExecuter.SqlType.GetTableHelper()!.Pagging(skip, take);
            var sql = $"SELECT * FROM {scriptExecuter.SqlType.Wrap(tableName)} {paggingPart}";
            scriptExecuter.Read(sql, handler);
        }
        public static Task ReadTableAllColumnsAsync(this IDbScriptExecuter scriptExecuter, string tableName, ReadDataHandlerSync handler,  CancellationToken token = default)
        {
            return scriptExecuter.ReadTableAllColumnsAsync(tableName, (o,e) =>
            {
                handler(o, e);
                return Task.CompletedTask;
            }, token: token);
        }
        public static Task ReadAsync(this IScriptExecuter scriptExecuter,string script, ReadDataHandlerSync handler,  object? args = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadAsync(script, (o, e) =>
            {
                handler(o, e);
                return Task.CompletedTask;
            }, args: PrepareArgs(args), token: token);
        }
        public static void Read(this IScriptExecuter scriptExecuter, string script, ReadDataHandlerSync handler, object? args = null)
        {
            scriptExecuter.Read(script, (o, e) =>
            {
                handler(o, e);
            }, args: PrepareArgs(args));
        }
        public static Task<T?> ReadOneAsync<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, static (o, e) =>
            {
                if (e.Reader.Read())
                {
                    var result = RecordToObjectManager<T>.RecordToObject.To(e.Reader);
                    return Task.FromResult(result);
                }
                return EmptyTaskResult<T>.EmptyResult;
            }, args: PrepareArgs(args), token: token);
        }
        public static T? ReadOne<T>(this IScriptExecuter scriptExecuter, string script, object? args = null)
        {
            return scriptExecuter.ReadResult(script, static (o, e) =>
            {
                if (e.Reader.Read())
                {
                    var result = RecordToObjectManager<T>.RecordToObject.To(e.Reader);
                    return result;
                }
                return default;
            }, args: PrepareArgs(args));
        }
        public static Task<IList<T?>> ReadRowsAsync<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, (o, e) =>
            {
                var res = RecordToObjectManager<T>.RecordToObject.ToList(e.Reader);
                return Task.FromResult(res);
            }, args: PrepareArgs(args), token: token);
        }
        public static IList<T?> ReadRows<T>(this IScriptExecuter scriptExecuter, string script, object? args = null)
        {
            return scriptExecuter.ReadResult(script,static (o, e) => RecordToObjectManager<T>.RecordToObject.ToList(e.Reader), args: PrepareArgs(args));
        }
        public static Task<IList<T?>> ReadRowsAsync<T>(this IScriptExecuter scriptExecuter, string script, Func<IDataReader, T?> reader, object? args = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, (o, e) =>
            {
                var res = new List<T?>();
                while (e.Reader.Read())
                {
                    res.Add(reader(e.Reader));
                }
                return Task.FromResult<IList<T?>>(res);
            }, args: PrepareArgs(args), token: token);
        }
        public static IList<T?> ReadRows<T>(this IScriptExecuter scriptExecuter, string script, Func<IDataReader, T?> reader, object? args = null)
        {
            return scriptExecuter.ReadResult(script, (o, e) =>
            {
                var res = new List<T?>();
                while (e.Reader.Read())
                {
                    res.Add(reader(e.Reader));
                }
                return res;
            }, args: PrepareArgs(args));
        }

        public static Task<IList<T?>> ReadAsync<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, static (o, e) => Task.FromResult(RecordToObjectManager<T>.ToList(e.Reader)), args: PrepareArgs(args), token: token);
        }
        public static IList<T?> Read<T>(this IScriptExecuter scriptExecuter, string script, object? args = null)
        {
            return scriptExecuter.ReadResult(script, static (o, e) => RecordToObjectManager<T>.ToList(e.Reader), args: PrepareArgs(args));
        }

        public static async IAsyncEnumerable<T?> EnumerableAsync<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, [EnumeratorCancellation] CancellationToken token = default)
        {
            using (var result = await scriptExecuter.ReadAsync(script, PrepareArgs(args), token))
            {
                var reader = result.Args.Reader;
                while (reader.Read())
                {
                    yield return result.Read<T?>();
                }
            }
        }
        public static IEnumerable<T?> Enumerable<T>(this IScriptExecuter scriptExecuter, string script, object? args = null)
        {
            using (var result = scriptExecuter.Read(script, PrepareArgs(args)))
            {
                var reader = result.Args.Reader;
                while (reader.Read())
                {
                    yield return result.Read<T?>();
                }
            }
        }

        public static Task EnumerableAsync<T>(this IScriptExecuter scriptExecuter, string script, Action<T?> receiver, object? args = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, (o, e) =>
            {
                foreach (var item in RecordToObjectManager<T>.Enumerable(e.Reader))
                {
                    receiver(item);
                }
                return Task.FromResult(false);
            }, args: PrepareArgs(args), token: token);
        }
        public static void Enumerable<T>(this IScriptExecuter scriptExecuter, string script, Action<T?> receiver, object? args = null)
        {
            scriptExecuter.ReadResult(script, (o, e) =>
            {
                foreach (var item in RecordToObjectManager<T>.Enumerable(e.Reader))
                {
                    receiver(item);
                }
                return false;
            }, args: PrepareArgs(args));
        }

        public static Task<int> ExecuteInterpolatedAsync(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p", CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ExecuteAsync(result.Sql, result.Arguments, token);
        }
        public static int ExecuteInterpolated(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p")
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.Execute(result.Sql, result.Arguments);
        }
        public static Task ReadInterpolatedAsync(this IDbScriptExecuter executer, ReadDataHandler handler, FormattableString f, string argPrefx = "p", CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ReadAsync(result.Sql, handler, result.Arguments, token);
        }
        public static void ReadInterpolated(this IDbScriptExecuter executer, ReadDataHandler handler, FormattableString f, string argPrefx = "p")
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            executer.ReadAsync(result.Sql, handler, result.Arguments);
        }
        public static Task ReadOneInterpolatedAsync<TResult>(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p", CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ReadOneAsync<TResult>(result.Sql, result.Arguments, token);
        }
        public static void ReadOneInterpolated<TResult>(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p")
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            executer.ReadOneAsync<TResult>(result.Sql, result.Arguments);
        }
        public static Task ReadOneInterpolatedAsync<TResult>(this IDbScriptExecuter executer, ReadDataResultHandler<TResult> handler, FormattableString f, string argPrefx = "p", CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ReadResultAsync(result.Sql, handler, result.Arguments, token);
        }
        public static void ReadOneInterpolated<TResult>(this IDbScriptExecuter executer, ReadDataResultHandler<TResult> handler, FormattableString f, string argPrefx = "p")
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            executer.ReadResultAsync(result.Sql, handler, result.Arguments);
        }
        public static Task ExistsInterpolatedAsync(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p", CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ExistsAsync(result.Sql, result.Arguments, token);
        }
        public static void ExistsInterpolated(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p")
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            executer.ExistsAsync(result.Sql, result.Arguments);
        }
        public static Task ExistsInterpolatedAsync<TResult>(this IDbScriptExecuter executer, Func<IDataReader, TResult?> reader, FormattableString f, string argPrefx = "p", CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ReadRowsAsync(result.Sql, reader, result.Arguments, token);
        }
        public static void ExistsInterpolated<TResult>(this IDbScriptExecuter executer, Func<IDataReader, TResult?> reader, FormattableString f, string argPrefx = "p")
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            executer.ReadRowsAsync(result.Sql, reader, result.Arguments);
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
