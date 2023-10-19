using System.Data;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public static class ScriptReadExecuterORMExtensions
    {
        public static async Task<bool> ExistsAsync(this IScriptExecuter scriptExecuter, string script, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default)
        {
            var exists = false;
            await scriptExecuter.ReadAsync(script, (o, e) =>
            {
                exists = e.Reader.Read();
                return Task.CompletedTask;
            }, args: args, token: token);
            return exists;
        }
        public static async Task<T?> ReadOneAsync<T>(this IScriptExecuter scriptExecuter, string script, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default)
        {
            T? result = default;
            await scriptExecuter.ReadAsync(script, (o, e) =>
            {
                Parser<T>.TryPreParse(e.Reader, out result);
                return Task.CompletedTask;
            }, args: args, token: token);
            return result;
        }
        public static async Task<IList<T?>> ReadRowsAsync<T>(this IScriptExecuter scriptExecuter, string script, Func<IDataReader, T?> reader, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default)
        {
            var res = new List<T?>();
            await scriptExecuter.ReadAsync(script, (o, e) =>
            {
                while (e.Reader.Read())
                {
                    res.Add(reader(e.Reader));
                }
                return Task.CompletedTask;
            }, args: args, token: token);
            return res;
        }
        public static async Task<IList<T>> ReadAsync<T>(this IScriptExecuter scriptExecuter, string script, IEnumerable<KeyValuePair<string, object?>>? args = null, CancellationToken token = default)
        {
            IList<T> result = Array.Empty<T>();
            await scriptExecuter.ReadAsync(script, (o, e) =>
            {
                result = Parser<T>.parser(e.Reader);
                return Task.CompletedTask;
            }, args: args, token: token);
            return result;
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
                    if (typeof(T) == typeof(int))
                    {
                        var dt = reader.GetInt32(0);
                        result = Unsafe.As<int, T>(ref dt);
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
