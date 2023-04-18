using DatabaseSchemaReader.DataSchema;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public class PaggingHelper
	{
        public static string Sql(int? skip,int? take, SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return SqlServer(skip, take);
                case SqlType.MySql:
                    return MySql(skip, take);
                case SqlType.SQLite:
                    return Sqlite(skip, take);
                case SqlType.PostgreSql:
                    return PostgreSql(skip,take);
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MySql(int? skip,int? take)
        {
            if (skip==null&&take==null)
            {
                return string.Empty;
            }
            if (skip!=null&&take!=null)
            {
                return $"LIMIT {skip}, {take}";
            }
            if (skip!=null)
            {
                return $"LIMIT {skip}";
            }
            return $"LIMIT 0,{skip}";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SqlServer(int? skip,int? take)
        {
            if (skip == null && take == null)
            {
                return string.Empty;
            }
            if (skip != null && take != null)
            {
                return $"OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY";
            }
            if (skip != null)
            {
                return $" OFFSET {skip} ROWS";
            }
            return $"OFFSET 0 ROWS FETCH NEXT {take} ROWS ONLY";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Sqlite(int? skip,int? take)
        {
            if (skip == null && take == null)
            {
                return string.Empty;
            }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string PostgreSql(int? skip,int? take)
        {
            if (skip == null && take == null)
            {
                return string.Empty;
            }
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
    }

}
