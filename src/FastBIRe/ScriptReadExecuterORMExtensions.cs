using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public static class ScriptReadExecuterORMExtensions
    {
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
            private static readonly Type ObjectType = typeof(Object);

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
                Parser<T>.TryPreParse(e.Reader, out var result);
                return Task.FromResult(result);
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
        public static Task<IList<T>> ReadAsync<T>(this IScriptExecuter scriptExecuter, string script, object? args = null, CancellationToken token = default)
        {
            return scriptExecuter.ReadResultAsync(script, static (o, e) => Task.FromResult(Parser<T>.parser(e.Reader)), args: PrepareArgs(args), token: token);
        }

        static class Parser<T>
        {
            internal static readonly Func<IDataReader, IList<T>> parser;

            internal static readonly Type type = typeof(T);
            internal static readonly Type actualType;

            internal static bool IsPreParse;

            public static bool TryPreParse(IDataReader reader, out T? result)
            {
                if (reader.Read() && reader.FieldCount > 0)
                {
                    if (reader.IsDBNull(0))
                    {
                        result = default;
                        return false;
                    }
                    if (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?))
                    {
                        var dt = reader.GetBoolean(0);
                        result = Unsafe.As<bool, T>(ref dt);
                        return true;
                    }
                    if (typeof(T) == typeof(byte) || typeof(T) == typeof(byte?))
                    {
                        var dt = reader.GetByte(0);
                        result = Unsafe.As<byte, T>(ref dt);
                        return true;
                    }
                    if (typeof(T) == typeof(short) || typeof(T) == typeof(short?))
                    {
                        var dt = reader.GetInt16(0);
                        result = Unsafe.As<short, T>(ref dt);
                        return true;
                    }
                    if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
                    {
                        var dt = reader.GetInt32(0);
                        result = Unsafe.As<int, T>(ref dt);
                        return true;
                    }
                    if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
                    {
                        var dt = reader.GetInt64(0);
                        result = Unsafe.As<long, T>(ref dt);
                        return true;
                    }
                    if (typeof(T) == typeof(float) || typeof(T) == typeof(float?))
                    {
                        var dt = reader.GetFloat(0);
                        result = Unsafe.As<float, T>(ref dt);
                        return true;
                    }
                    if (typeof(T) == typeof(double) || typeof(T) == typeof(double?))
                    {
                        var dt = reader.GetDouble(0);
                        result = Unsafe.As<double, T>(ref dt);
                        return true;
                    }
                    if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
                    {
                        var dt = reader.GetDecimal(0);
                        result = Unsafe.As<decimal, T>(ref dt);
                        return true;
                    }
                    if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
                    {
                        var dt = reader.GetDateTime(0);
                        result = (T)(object)dt;
                        return true;
                    }
                    if (typeof(T) == typeof(string))
                    {
                        var dt = reader.GetString(0);
                        result = Unsafe.As<string, T>(ref dt);
                        return true;
                    }
                    if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?))
                    {
                        var dt = reader.GetGuid(0);
                        result = Unsafe.As<Guid, T>(ref dt);
                        return true;
                    }
                    var res = reader.GetValue(0);
                    result = (T)Convert.ChangeType(res, actualType);
                    return true;
                }
                result = default;
                return false;
            }

            static Parser()
            {
                actualType = type;
                IsPreParse = type.IsPrimitive || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) || type == typeof(string) || type == typeof(decimal);
                if (IsPreParse)
                {
                    actualType = type.IsGenericType ? type.GetGenericArguments()[0] : type;
                    parser = reader =>
                    {
                        if (TryPreParse(reader, out var result))
                        {
                            return new[] { result! };
                        }
                        return Array.Empty<T>();
                    };
                }
                else if (type.IsClass)
                {
                    parser = ObjectMapper<T>.FillList;
                }
                else
                {
                    throw new NotSupportedException($"Type {typeof(T)} not support!");
                }
            }
        }
    }
}
