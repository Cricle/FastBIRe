namespace FastBIRe
{
    public static class TableWrapperInvokeExtensions
    {
        public static Task<int> InsertOrUpdateAsync(this TableWrapper wrapper, IScriptExecuter executer, IEnumerable<object?> values, CancellationToken token = default)
        {
            var sql = wrapper.CreateInsertOrUpdate(values);
            ThrowIfSqlNull(sql);
            return executer.ExecuteAsync(sql!, token: token);
        }
        public static Task<int> InsertAsync(this TableWrapper wrapper, IScriptExecuter executer, IEnumerable<object?> values, CancellationToken token = default)
        {
            var sql = wrapper.CreateInsertSql(values);
            ThrowIfSqlNull(sql);
            return executer.ExecuteAsync(sql!, token: token);
        }
        public static Task<int> DeleteByKeyAsync(this TableWrapper wrapper, IScriptExecuter executer, IEnumerable<object?> values, CancellationToken token = default)
        {
            var sql = wrapper.CreateDeleteByKeySql(values);
            ThrowIfSqlNull(sql);
            return executer.ExecuteAsync(sql!, token: token);
        }
        public static async Task<int> DeleteByKeyManyAsync(this TableWrapper wrapper, IScriptExecuter executer, IEnumerable<IEnumerable<object?>> values, CancellationToken token = default)
        {
            var res = 0;
            foreach (var item in values)
            {
                res += await DeleteByKeyAsync(wrapper, executer, item, token);
            }
            return res;
        }
        public static async Task<int> InsertOrUpdateManyAsync(this TableWrapper wrapper, IScriptExecuter executer, IEnumerable<IEnumerable<object?>> values, CancellationToken token = default)
        {
            var res = 0;
            foreach (var item in values)
            {
                res += await InsertOrUpdateAsync(wrapper, executer, item, token);
            }
            return res;
        }
        public static async Task<int> InsertManyAsync(this TableWrapper wrapper, IScriptExecuter executer, IEnumerable<IEnumerable<object?>> values, CancellationToken token = default)
        {
            var res = 0;
            foreach (var item in values)
            {
                res += await InsertAsync(wrapper, executer, item, token);
            }
            return res;
        }
        private static void ThrowIfSqlNull(string? sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException("The generate sql is null");
            }
        }
    }
}
