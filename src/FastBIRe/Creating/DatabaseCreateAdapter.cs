using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Creating
{
    public class DatabaseCreateAdapter : IDatabaseCreateAdapter
    {
        public static readonly DatabaseCreateAdapter MySql = new DatabaseCreateAdapter(@"CREATE DATABASE `{0}`;",
            @"CREATE DATABASE IF NOT EXISTS `{0}`;",
            @"DROP DATABASE `{0}`;",
            @"DROP DATABASE IF EXISTS `{0}`;",
            @"DROP TABLE `{0}`;",
            @"DROP TABLE IF EXISTS `{0}`;",
            @"SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}';");
        public static readonly DatabaseCreateAdapter SqlServer = new DatabaseCreateAdapter(@" CREATE DATABASE [{0}];",
            @"IF NOT EXISTS(SELECT [name] FROM [sys].[databases] WHERE [name] = '{0}') CREATE DATABASE [{0}];",
            @"DROP DATABASE [{0}];",
            @"
IF DB_ID('your_database_name') IS NOT NULL
BEGIN
  ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
  DROP DATABASE [{0}];
END",
            @"DROP TABLE [{0}];",
            @"DROP TABLE IF EXISTS [{0}];",
            @"SELECT 1 FROM [sys].[databases] WHERE [name] = '{0}';");
        public static readonly DatabaseCreateAdapter MariaDB = MySql;
        public static readonly DatabaseCreateAdapter Sqlite = new DatabaseCreateAdapter(@"SELECT true",
            @"SELECT true",
            string.Empty,
            string.Empty,
            @"DROP TABLE ""{0}"";",
            @"DROP TABLE IF EXISTS ""{0}"";",
            @"SELECT 1;");
        public static readonly DatabaseCreateAdapter DuckDb = new DatabaseCreateAdapter(@"SELECT true",
            @"SELECT true",
            string.Empty,
            string.Empty,
            @"DROP TABLE ""{0}"";",
            @"DROP TABLE IF EXISTS ""{0}"";",
            @"SELECT 1;");

        public static readonly DatabaseCreateAdapter Oracle = new DatabaseCreateAdapter(@"CREATE DATABASE {0};", @"
DECLARE
  db_count NUMBER := 0;
BEGIN
  SELECT COUNT(*) INTO db_count FROM dba_users WHERE username = '{0}';
  IF db_count = 0 THEN
    EXECUTE IMMEDIATE 'CREATE USER {0} IDENTIFIED BY password';
    EXECUTE IMMEDIATE 'GRANT CREATE SESSION, CREATE TABLE, CREATE SEQUENCE TO ""{0}""';
  END IF;
END;",
            @"DROP DATABASE {0};",
            @"
DECLARE
  db_count NUMBER := 0;
BEGIN
  SELECT COUNT(*) INTO db_count FROM dba_users WHERE username = '{0}';
  IF db_count > 0 THEN
    EXECUTE IMMEDIATE 'DROP USER {0} CASCADE';
  END IF;
END;",
            @"DROP TABLE ""{0}"";",
            @"DROP TABLE ""{0}"";",
            @"IF DB_ID('database_name') IS NOT NULL
BEGIN
    SELECT 1;
END
ELSE
BEGIN
    SELECT 0;
END;");
        public static readonly DatabaseCreateAdapter PostgreSql = new DatabaseCreateAdapter(@"CREATE DATABASE ""{0}"";", @"
DO $$ 
BEGIN
  IF NOT EXISTS (SELECT FROM pg_database WHERE datname = '{0}') THEN
    CREATE DATABASE ""{0}"";
  END IF;
END $$;",
            @"DROP DATABASE ""{0}"";",
            @"DROP DATABASE IF EXISTS ""{0}"";",
            @"DROP TABLE ""{0}"";",
            @"DROP TABLE IF EXISTS ""{0}"";",
            @"SELECT 1 FROM pg_database WHERE datname = '{0}';");

        public DatabaseCreateAdapter(string createDatabaseSqlFormatSqlFormat,
            string createDatabaseSqlFormatIfNotExistsFormat,
            string dropSqlFormat,
            string dropDatabaseSqlFormatIfExistsFormat,
            string dropTableSqlFormatSqlFormat,
            string dropTableSqlFormatIfExistsFormat,
            string checkDatabaseExistsSqlFormat)
        {
            CreateDatabaseSqlFormat = createDatabaseSqlFormatSqlFormat ?? throw new ArgumentNullException(nameof(createDatabaseSqlFormatSqlFormat));
            CreateDatabaseSqlFormatIfNotExistsFormat = createDatabaseSqlFormatIfNotExistsFormat ?? throw new ArgumentNullException(nameof(createDatabaseSqlFormatIfNotExistsFormat));
            DropDatabaseSqlFormatSqlFormat = dropSqlFormat ?? throw new ArgumentNullException(nameof(dropSqlFormat));
            DropDatabaseSqlFormatIfExistsFormat = dropDatabaseSqlFormatIfExistsFormat ?? throw new ArgumentNullException(nameof(dropDatabaseSqlFormatIfExistsFormat));
            DropTableSqlFormatSqlFormat = dropTableSqlFormatSqlFormat;
            DropTableSqlFormatIfExistsFormat = dropTableSqlFormatIfExistsFormat;
            CheckDatabaseExistsSqlFormat = checkDatabaseExistsSqlFormat;
        }

        public string CreateDatabaseSqlFormat { get; }

        public string CreateDatabaseSqlFormatIfNotExistsFormat { get; }

        public string DropDatabaseSqlFormatSqlFormat { get; }

        public string DropDatabaseSqlFormatIfExistsFormat { get; }

        public string DropTableSqlFormatSqlFormat { get; }

        public string DropTableSqlFormatIfExistsFormat { get; }

        public string CheckDatabaseExistsSqlFormat { get; }

        public string CheckDatabaseExists(string database)
        {
            return string.Format(CheckDatabaseExistsSqlFormat, database);
        }
        public string CreateDatabase(string database)
        {
            return string.Format(CreateDatabaseSqlFormat, database);
        }
        public string CreateDatabaseIfNotExists(string database)
        {
            return string.Format(CreateDatabaseSqlFormatIfNotExistsFormat, database);
        }

        public string DropDatabase(string database)
        {
            return string.Format(DropDatabaseSqlFormatSqlFormat, database);
        }

        public string DropDatabaseIfExists(string database)
        {
            return string.Format(DropDatabaseSqlFormatIfExistsFormat, database);
        }

        public string DropTable(string table)
        {
            return string.Format(DropTableSqlFormatSqlFormat, table);
        }

        public string DropTableIfExists(string table)
        {
            return string.Format(DropTableSqlFormatIfExistsFormat, table);
        }
        public static DatabaseCreateAdapter? Get(SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return SqlServer;
                case SqlType.Oracle:
                    return Oracle;
                case SqlType.MySql:
                    return MySql;
                case SqlType.SQLite:
                    return Sqlite;
                case SqlType.PostgreSql:
                    return PostgreSql;
                case SqlType.DuckDB:
                    return DuckDb;
                default:
                    return null;
            }
        }
    }

}
