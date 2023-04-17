namespace FastBIRe
{
    public interface ISQLDatabaseCreateAdapter
    {
        string GenericCreateDatabaseSql(string database);

        string GenericCreateDatabaseIfNotExistsSql(string database);

        string GenericDropDatabaseSql(string database);

        string GenericDropDatabaseIfExistsSql(string database);

        string GenericDropTableSql(string database);

        string GenericDropTableIfExistsSql(string database);
    }
    public class SQLDatabaseCreateAdapter : ISQLDatabaseCreateAdapter
    {
        public static readonly SQLDatabaseCreateAdapter MySql = new SQLDatabaseCreateAdapter("CREATE DATABASE `{0}`;", "CREATE DATABASE IF NOT EXISTS `{0}`;", "DROP DATABASE `{0}`;", "DROP DATABASE IF EXISTS `{0}`;", "DROP TABLE `{0}`;", "DROP TABLE IF EXISTS `{0}`;");

        public static readonly SQLDatabaseCreateAdapter SqlServer = new SQLDatabaseCreateAdapter(" CREATE DATABASE [{0}];", "IF NOT EXISTS(SELECT [name] FROM [sys].[databases] WHERE [name] = '{0}') CREATE DATABASE [{0}];", "DROP DATABASE [{0}];", "\r\nIF DB_ID('your_database_name') IS NOT NULL\r\nBEGIN\r\n  ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;\r\n  DROP DATABASE [{0}];\r\nEND", "DROP TABLE [{0}];", "DROP TABLE IF EXISTS [{0}];");

        public static readonly SQLDatabaseCreateAdapter MariaDB = MySql;

        public static readonly SQLDatabaseCreateAdapter Sqlite = new SQLDatabaseCreateAdapter("SELECT true", "SELECT true", string.Empty, string.Empty, "DROP TABLE \"{0}\";", "DROP TABLE IF EXISTS \"{0}\";");

        public static readonly SQLDatabaseCreateAdapter Oracle = new SQLDatabaseCreateAdapter("CREATE DATABASE {0};", "\r\nDECLARE\r\n  db_count NUMBER := 0;\r\nBEGIN\r\n  SELECT COUNT(*) INTO db_count FROM dba_users WHERE username = '{0}';\r\n  IF db_count = 0 THEN\r\n    EXECUTE IMMEDIATE 'CREATE USER {0} IDENTIFIED BY password';\r\n    EXECUTE IMMEDIATE 'GRANT CREATE SESSION, CREATE TABLE, CREATE SEQUENCE TO \"{0}\"';\r\n  END IF;\r\nEND;", "DROP DATABASE {0};", "\r\nDECLARE\r\n  db_count NUMBER := 0;\r\nBEGIN\r\n  SELECT COUNT(*) INTO db_count FROM dba_users WHERE username = '{0}';\r\n  IF db_count > 0 THEN\r\n    EXECUTE IMMEDIATE 'DROP USER {0} CASCADE';\r\n  END IF;\r\nEND;", "DROP TABLE \"{0}\";", "DROP TABLE \"{0}\";");

        public static readonly SQLDatabaseCreateAdapter PostgreSql = new SQLDatabaseCreateAdapter("CREATE DATABASE \"{0}\";", "\r\nDO $$ \r\nBEGIN\r\n  IF NOT EXISTS (SELECT FROM pg_database WHERE datname = '{0}') THEN\r\n    CREATE DATABASE \"{0}\";\r\n  END IF;\r\nEND $$;", "DROP DATABASE \"{0}\";", "DROP DATABASE IF EXISTS \"{0}\";", "DROP TABLE \"{0}\";", "DROP TABLE IF EXISTS \"{0}\";");

        public string CreateDatabaseSqlFormat { get; }

        public string CreateDatabaseSqlFormatIfNotExistsFormat { get; }

        public string DropDatabaseSqlFormatSqlFormat { get; }

        public string DropDatabaseSqlFormatIfExistsFormat { get; }

        public string DropTableSqlFormatSqlFormat { get; }

        public string DropTableSqlFormatIfExistsFormat { get; }

        public SQLDatabaseCreateAdapter(string createDatabaseSqlFormatSqlFormat, string createDatabaseSqlFormatIfNotExistsFormat, string dropSqlFormat, string dropDatabaseSqlFormatIfExistsFormat, string dropTableSqlFormatSqlFormat, string dropTableSqlFormatIfExistsFormat)
        {
            CreateDatabaseSqlFormat = createDatabaseSqlFormatSqlFormat ?? throw new ArgumentNullException("createDatabaseSqlFormatSqlFormat");
            CreateDatabaseSqlFormatIfNotExistsFormat = createDatabaseSqlFormatIfNotExistsFormat ?? throw new ArgumentNullException("createDatabaseSqlFormatIfNotExistsFormat");
            DropDatabaseSqlFormatSqlFormat = dropSqlFormat ?? throw new ArgumentNullException("dropSqlFormat");
            DropDatabaseSqlFormatIfExistsFormat = dropDatabaseSqlFormatIfExistsFormat ?? throw new ArgumentNullException("dropDatabaseSqlFormatIfExistsFormat");
            DropTableSqlFormatSqlFormat = dropTableSqlFormatSqlFormat;
            DropTableSqlFormatIfExistsFormat = dropTableSqlFormatIfExistsFormat;
        }

        public string GenericCreateDatabaseSql(string database)
        {
            return string.Format(CreateDatabaseSqlFormat, database);
        }

        public string GenericCreateDatabaseIfNotExistsSql(string database)
        {
            return string.Format(CreateDatabaseSqlFormatIfNotExistsFormat, database);
        }

        public string GenericDropDatabaseSql(string database)
        {
            return string.Format(DropDatabaseSqlFormatSqlFormat, database);
        }

        public string GenericDropDatabaseIfExistsSql(string database)
        {
            return string.Format(DropDatabaseSqlFormatIfExistsFormat, database);
        }

        public string GenericDropTableSql(string table)
        {
            return string.Format(DropTableSqlFormatSqlFormat, table);
        }

        public string GenericDropTableIfExistsSql(string table)
        {
            return string.Format(DropTableSqlFormatIfExistsFormat, table);
        }
    }
}
