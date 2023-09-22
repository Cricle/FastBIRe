using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public static class PaggingHelper
    {
        public static string Sql(int? skip, int? take, SqlType sqlType)
        {
            if (skip == null && take == null)
            {
                return string.Empty;
            }
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    {
                        if (skip != null && take != null)
                        {
                            return $"OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY";
                        }
                        if (skip != null)
                        {
                            return $"OFFSET {skip} ROWS";
                        }
                        return $"OFFSET 0 ROWS FETCH NEXT {take} ROWS ONLY";
                    }
                case SqlType.MySql:
                    {
                        if (skip != null && take != null)
                        {
                            return $"LIMIT {skip}, {take}";
                        }
                        if (skip != null)
                        {
                            return $"LIMIT {skip}";
                        }
                        return $"LIMIT 0, {take}";
                    }
                case SqlType.SQLite:
                    {
                        if (skip != null && take != null)
                        {
                            return $"LIMIT {take} OFFSET {skip}";
                        }
                        if (skip != null)
                        {
                            return $"LIMIT -1 OFFSET {skip}";
                        }
                        return $"LIMIT {take} OFFSET 0";
                    }
                case SqlType.PostgreSql:
                    {
                        if (skip != null && take != null)
                        {
                            return $"OFFSET {skip} LIMIT {take}";
                        }
                        if (skip != null)
                        {
                            return $"OFFSET {skip}";
                        }
                        return $"LIMIT {take}";
                    }
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
        }
    }

}
