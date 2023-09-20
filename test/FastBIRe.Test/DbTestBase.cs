using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using System.Data.Common;

namespace FastBIRe.Test
{
    public abstract class DbTestBase
    {
        public string Quto(SqlType type,string name)
        {
            return MergeHelper.Wrap(type, name);
        }

        protected readonly DatabaseIniter databaseIniter= DatabaseIniter.Instance;

    }
}
