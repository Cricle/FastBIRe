using DatabaseSchemaReader.DataSchema;
using System.Collections;
using System.Collections.Concurrent;
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
            return Enumerable.Repeat(new KeyValuePair<string, object?>(string.Empty, obj), 1);
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
        public static Task<IList<T?>> ReadRowsAsync<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, (o, e) =>
            {
                var res = RecordToObjectManager<T>.RecordToObject.ToList(e.Reader);
                return Task.FromResult(res);
            }, args: PrepareArgs(args), token: token);
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

        public static Task<IList<T?>> ReadAsync<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, static (o, e) => Task.FromResult(RecordToObjectManager<T>.ToList(e.Reader)), args: PrepareArgs(args), token: token);
        }

        public static Task<int> ExecuteInterpolatedAsync(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p", CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ExecuteAsync(result.Sql, result.Arguments, token);
        }
        public static Task ReadInterpolatedAsync(this IDbScriptExecuter executer, ReadDataHandler handler, FormattableString f, string argPrefx = "p", CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ReadAsync(result.Sql, handler, result.Arguments, token);
        }
        public static Task ReadOneInterpolatedAsync<TResult>(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p", CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ReadOneAsync<TResult>(result.Sql, result.Arguments, token);
        }
        public static Task ReadOneInterpolatedAsync<TResult>(this IDbScriptExecuter executer, ReadDataResultHandler<TResult> handler, FormattableString f, string argPrefx = "p", CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ReadResultAsync(result.Sql, handler, result.Arguments, token);
        }
        public static Task ExistsInterpolatedAsync(this IDbScriptExecuter executer, FormattableString f, string argPrefx = "p", CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ExistsAsync(result.Sql, result.Arguments, token);
        }
        public static Task ExistsInterpolatedAsync<TResult>(this IDbScriptExecuter executer, Func<IDataReader, TResult?> reader, FormattableString f, string argPrefx = "p", CancellationToken token = default)
        {
            var result = InterpolatedHelper.Parse(executer.SqlType, f, argPrefx);
            return executer.ReadRowsAsync(result.Sql, reader, result.Arguments, token);
        }
    }
}
