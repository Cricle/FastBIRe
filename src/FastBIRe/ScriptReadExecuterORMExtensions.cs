using System.Data;

namespace FastBIRe
{
    public static class ScriptReadExecuterORMExtensions
    {
        public static async Task<IList<T>> ReadAsync<T>(this IScriptExecuter scriptExecuter,string script, CancellationToken token = default)
        {
            IList<T> result = Array.Empty<T>();
            await scriptExecuter.ReadAsync(script, (o, e) =>
            {
                result = Parser<T>.parser(e.Reader);
                return Task.CompletedTask;
            },token);
            return result;
        }

        static class Parser<T>
        {
            internal static readonly Func<IDataReader, IList<T>> parser;

            internal static readonly Type type = typeof(T);

            static Parser()
            {
                if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                {
                    parser = reader =>
                    {
                        if (reader.Read() && reader.FieldCount > 0)
                        {
                            var val = reader.GetValue(0);
                            if (val != DBNull.Value)
                            {
                                var res = (T)Convert.ChangeType(val, type);
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
