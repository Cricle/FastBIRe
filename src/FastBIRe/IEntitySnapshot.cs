using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public interface IEntitySnapshot<T> : IEntityColumnsSnapshot
    {
        string CreateDeleteByKeySql(SqlType sqlType, string tableName, T instance);

        string CreateUpdateByKeySql(SqlType sqlType, string tableName, T instance);

        string CreateInsertSql(SqlType sqlType, string tableName, T instance, bool skipAutoNumber = true);
    }
}
