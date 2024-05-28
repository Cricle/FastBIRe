using FastBIRe.Wrapping;

namespace FastBIRe.Test.Wrapping
{
    [TestClass]
    public class DefaultEscaperTest
    {
        private IEscaper GetEscaper(SqlType sqlType)
        {
            IEscaper escaper = null!;
            switch (sqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    escaper = DefaultEscaper.SqlServer;
                    break;
                case SqlType.MySql:
                    escaper = DefaultEscaper.MySql;
                    break;
                case SqlType.SQLite:
                    escaper = DefaultEscaper.Sqlite;
                    break;
                case SqlType.PostgreSql:
                    escaper = DefaultEscaper.PostgreSql;
                    break;
                case SqlType.DuckDB:
                    escaper = DefaultEscaper.DuckDB;
                    break;
            }
            return escaper;
        }

        [TestMethod]
        [DataRow(SqlType.SqlServer, "field", "[field]")]
        [DataRow(SqlType.SqlServerCe, "field", "[field]")]
        [DataRow(SqlType.MySql, "field", "`field`")]
        [DataRow(SqlType.SQLite, "field", "`field`")]
        [DataRow(SqlType.PostgreSql, "field", "\"field\"")]
        [DataRow(SqlType.DuckDB, "field", "\"field\"")]
        public void Quto(SqlType sqlType, string input, string exp)
        {
            var escaper = GetEscaper(sqlType);
            Assert.AreEqual(exp, escaper!.Quto(input));
        }
        [TestMethod]
        [DataRow(SqlType.SqlServer)]
        [DataRow(SqlType.SqlServerCe)]
        [DataRow(SqlType.MySql)]
        [DataRow(SqlType.SQLite)]
        [DataRow(SqlType.PostgreSql)]
        [DataRow(SqlType.DuckDB)]
        public void WrapValueNULL(SqlType sqlType)
        {
            var escaper = GetEscaper(sqlType);
            Assert.AreEqual("NULL", escaper!.WrapValue<object>(null));
            Assert.AreEqual("NULL", escaper!.WrapValue(DBNull.Value));
        }
        [TestMethod]
        [DataRow(SqlType.SqlServer)]
        [DataRow(SqlType.SqlServerCe)]
        [DataRow(SqlType.MySql)]
        [DataRow(SqlType.SQLite)]
        [DataRow(SqlType.PostgreSql)]
        [DataRow(SqlType.DuckDB)]
        public void WrapValueString(SqlType sqlType)
        {
            var escaper = GetEscaper(sqlType);
            Assert.AreEqual("'hello world'", escaper!.WrapValue("hello world"));
        }
        [TestMethod]
        [DataRow(SqlType.SqlServer)]
        [DataRow(SqlType.SqlServerCe)]
        [DataRow(SqlType.MySql)]
        [DataRow(SqlType.SQLite)]
        [DataRow(SqlType.PostgreSql)]
        [DataRow(SqlType.DuckDB)]
        public void WrapValueStringWithQutoMask(SqlType sqlType)
        {
            var escaper = GetEscaper(sqlType);
            Assert.AreEqual("'hello'' world'", escaper!.WrapValue("hello' world"));
        }
        [TestMethod]
        public void MySqlWrapValueStringWithQuto()
        {
            var escaper = GetEscaper(SqlType.MySql);
            Assert.AreEqual("'\\\\a'", escaper!.WrapValue("\\a"));
        }
        [TestMethod]
        [DataRow(SqlType.SqlServer)]
        [DataRow(SqlType.SqlServerCe)]
        [DataRow(SqlType.MySql)]
        [DataRow(SqlType.SQLite)]
        [DataRow(SqlType.PostgreSql)]
        [DataRow(SqlType.DuckDB)]
        public void WrapValueGuid(SqlType sqlType)
        {
            var escaper = GetEscaper(sqlType);
            var guid = Guid.Parse("94F80767-3B2C-483C-A588-4D360910FBFA");
            Assert.AreEqual($"'{guid}'", escaper!.WrapValue(guid));
        }
        [TestMethod]
        [DataRow(SqlType.SqlServer)]
        [DataRow(SqlType.SqlServerCe)]
        [DataRow(SqlType.MySql)]
        [DataRow(SqlType.SQLite)]
        [DataRow(SqlType.PostgreSql)]
        [DataRow(SqlType.DuckDB)]
        public void WrapValueDateTime(SqlType sqlType)
        {
            var escaper = GetEscaper(sqlType);
            var dt = new DateTime(2023, 9, 26, 22, 23, 24);
            Assert.AreEqual("'2023-09-26 22:23:24'", escaper!.WrapValue(dt));
            dt = new DateTime(2023, 9, 26, 0, 0, 0);
            Assert.AreEqual("'2023-09-26'", escaper!.WrapValue(dt));
        }
        [TestMethod]
        [DataRow(SqlType.SqlServer)]
        [DataRow(SqlType.SqlServerCe)]
        [DataRow(SqlType.MySql)]
        [DataRow(SqlType.SQLite)]
        [DataRow(SqlType.PostgreSql)]
        [DataRow(SqlType.DuckDB)]
        public void WrapValueBytes(SqlType sqlType)
        {
            var escaper = GetEscaper(sqlType);
            var buffers = new byte[] { 1, 2, 3, 0xFF };
            Assert.AreEqual("0x010203FF", escaper!.WrapValue(buffers));
        }
        [TestMethod]
        [DataRow(SqlType.SqlServer, true, "1")]
        [DataRow(SqlType.SqlServer, false, "0")]
        [DataRow(SqlType.SqlServerCe, true, "1")]
        [DataRow(SqlType.SqlServerCe, false, "0")]
        [DataRow(SqlType.MySql, true, "1")]
        [DataRow(SqlType.MySql, false, "0")]
        [DataRow(SqlType.SQLite, true, "1")]
        [DataRow(SqlType.SQLite, false, "0")]
        [DataRow(SqlType.PostgreSql, true, "true")]
        [DataRow(SqlType.PostgreSql, false, "false")]
        [DataRow(SqlType.DuckDB, true, "true")]
        [DataRow(SqlType.DuckDB, false, "false")]
        public void WrapValueBoolean(SqlType sqlType, bool val, string act)
        {
            var escaper = GetEscaper(sqlType);
            Assert.AreEqual(act, escaper!.WrapValue(val));
        }
    }
}
