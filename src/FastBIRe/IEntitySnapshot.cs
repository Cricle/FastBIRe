namespace FastBIRe
{
    public interface IEntitySnapshot<T> : IEntityColumnsSnapshot
    {
        string CreateDeleteByKeySql(FSqlType sqlType, string tableName, T instance);

        string CreateUpdateByKeySql(FSqlType sqlType, string tableName, T instance);

        string CreateInsertSql(FSqlType sqlType, string tableName, T instance, bool skipAutoNumber = true);
    }
}
