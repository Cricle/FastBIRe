using System.Data;

namespace FastBIRe
{
    public static class ScriptReadExecuterORMExtensions
    {
        public static async Task<bool> ExistsAsync(this IScriptExecuter scriptExecuter, string script, CancellationToken token = default)
        {
            var exists = false;
            await scriptExecuter.ReadAsync(script, (o, e) =>
            {
                exists = e.Reader.Read();
                return Task.CompletedTask;
            }, token);
            return exists;
        }
        public static async Task<T?> ReadOneAsync<T>(this IScriptExecuter scriptExecuter, string script, CancellationToken token = default)
        {
            T? result = default;
            await scriptExecuter.ReadAsync(script, (o, e) =>
            {
                var lst = Parser<T>.parser(e.Reader);
                if (lst.Count != 0)
                {
                    result = lst[0];
                }
                return Task.CompletedTask;
            }, token);
            return result;
        }
        public static async Task<IList<T>> ReadAsync<T>(this IScriptExecuter scriptExecuter, string script, CancellationToken token = default)
        {
            IList<T> result = Array.Empty<T>();
            await scriptExecuter.ReadAsync(script, (o, e) =>
            {
                result = Parser<T>.parser(e.Reader);
                return Task.CompletedTask;
            }, token);
            return result;
        }

        static class Parser<T>
        {
            internal static readonly Func<IDataReader, IList<T>> parser;

            internal static readonly Type type = typeof(T);

            static Parser()
            {
                if (type.IsPrimitive||(type.IsGenericType&&type.GetGenericTypeDefinition()==typeof(Nullable<>)) || type == typeof(string) || type == typeof(decimal))
                {
                    var actualType = type.IsGenericType ? type.GetGenericArguments()[0] : type;
                    parser = reader =>
                    {
                        if (reader.Read() && reader.FieldCount > 0)
                        {
                            var val = reader.GetValue(0);
                            if (val != DBNull.Value)
                            {
                                var res = (T)Convert.ChangeType(val, actualType);
                                return new T[] { res };
                            }
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
