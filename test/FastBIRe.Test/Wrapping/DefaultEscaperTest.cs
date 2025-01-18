using FastBIRe.Wrapping;

namespace FastBIRe.Test.Wrapping
{
    [TestClass]
    public class DefaultEscaperTest
    {
        private IEscaper GetEscaper(FSqlType sqlType)
        {
            IEscaper escaper = null!;
            switch (sqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                    escaper = DefaultEscaper.SqlServer;
                    break;
                case FSqlType.MySql:
                    escaper = DefaultEscaper.MySql;
                    break;
                case FSqlType.SQLite:
                    escaper = DefaultEscaper.Sqlite;
                    break;
                case FSqlType.PostgreSql:
                    escaper = DefaultEscaper.PostgreSql;
                    break;
                case FSqlType.DuckDB:
                    escaper = DefaultEscaper.DuckDB;
                    break;
            }
            return escaper;
        }

        [TestMethod]
        [DataRow(FSqlType.SqlServer, "field", "[field]")]
        [DataRow(FSqlType.SqlServerCe, "field", "[field]")]
        [DataRow(FSqlType.MySql, "field", "`field`")]
        [DataRow(FSqlType.SQLite, "field", "`field`")]
        [DataRow(FSqlType.PostgreSql, "field", "\"field\"")]
        [DataRow(FSqlType.DuckDB, "field", "\"field\"")]
        public void Quto(FSqlType sqlType, string input, string exp)
        {
            var escaper = GetEscaper(sqlType);
            Assert.AreEqual(exp, escaper!.Quto(input));
        }
        [TestMethod]
        [DataRow(FSqlType.SqlServer)]
        [DataRow(FSqlType.SqlServerCe)]
        [DataRow(FSqlType.MySql)]
        [DataRow(FSqlType.SQLite)]
        [DataRow(FSqlType.PostgreSql)]
        [DataRow(FSqlType.DuckDB)]
        public void WrapValueNULL(FSqlType sqlType)
        {
            var escaper = GetEscaper(sqlType);
            Assert.AreEqual("NULL", escaper!.WrapValue<object>(null));
            Assert.AreEqual("NULL", escaper!.WrapValue(DBNull.Value));
        }
        [TestMethod]
        [DataRow(FSqlType.SqlServer)]
        [DataRow(FSqlType.SqlServerCe)]
        [DataRow(FSqlType.MySql)]
        [DataRow(FSqlType.SQLite)]
        [DataRow(FSqlType.PostgreSql)]
        [DataRow(FSqlType.DuckDB)]
        public void WrapValueString(FSqlType sqlType)
        {
            var escaper = GetEscaper(sqlType);
            Assert.AreEqual("'hello world'", escaper!.WrapValue("hello world"));
        }
        [TestMethod]
        [DataRow(FSqlType.SqlServer)]
        [DataRow(FSqlType.SqlServerCe)]
        [DataRow(FSqlType.MySql)]
        [DataRow(FSqlType.SQLite)]
        [DataRow(FSqlType.PostgreSql)]
        [DataRow(FSqlType.DuckDB)]
        public void WrapValueStringWithQutoMask(FSqlType sqlType)
        {
            var escaper = GetEscaper(sqlType);
            Assert.AreEqual("'hello'' world'", escaper!.WrapValue("hello' world"));
        }
        [TestMethod]
        public void MySqlWrapValueStringWithQuto()
        {
            var escaper = GetEscaper(FSqlType.MySql);
            Assert.AreEqual("'\\\\a'", escaper!.WrapValue("\\a"));
        }
        [TestMethod]
        [DataRow(FSqlType.SqlServer)]
        [DataRow(FSqlType.SqlServerCe)]
        [DataRow(FSqlType.MySql)]
        [DataRow(FSqlType.SQLite)]
        [DataRow(FSqlType.PostgreSql)]
        [DataRow(FSqlType.DuckDB)]
        public void WrapValueGuid(FSqlType sqlType)
        {
            var escaper = GetEscaper(sqlType);
            var guid = Guid.Parse("94F80767-3B2C-483C-A588-4D360910FBFA");
            Assert.AreEqual($"'{guid}'", escaper!.WrapValue(guid));
        }
        [TestMethod]
        [DataRow(FSqlType.SqlServer)]
        [DataRow(FSqlType.SqlServerCe)]
        [DataRow(FSqlType.MySql)]
        [DataRow(FSqlType.SQLite)]
        [DataRow(FSqlType.PostgreSql)]
        [DataRow(FSqlType.DuckDB)]
        public void WrapValueDateTime(FSqlType sqlType)
        {
            var escaper = GetEscaper(sqlType);
            var dt = new DateTime(2023, 9, 26, 22, 23, 24);
            Assert.AreEqual("'2023-09-26 22:23:24'", escaper!.WrapValue(dt));
            dt = new DateTime(2023, 9, 26, 0, 0, 0);
            Assert.AreEqual("'2023-09-26'", escaper!.WrapValue(dt));
        }
        [TestMethod]
        [DataRow(FSqlType.SqlServer)]
        [DataRow(FSqlType.SqlServerCe)]
        [DataRow(FSqlType.MySql)]
        [DataRow(FSqlType.SQLite)]
        [DataRow(FSqlType.PostgreSql)]
        [DataRow(FSqlType.DuckDB)]
        public void WrapValueBytes(FSqlType sqlType)
        {
            var escaper = GetEscaper(sqlType);
            var buffers = new byte[] { 1, 2, 3, 0xFF };
            Assert.AreEqual("0x010203FF", escaper!.WrapValue(buffers));
        }
        [TestMethod]
        [DataRow(FSqlType.SqlServer, true, "1")]
        [DataRow(FSqlType.SqlServer, false, "0")]
        [DataRow(FSqlType.SqlServerCe, true, "1")]
        [DataRow(FSqlType.SqlServerCe, false, "0")]
        [DataRow(FSqlType.MySql, true, "1")]
        [DataRow(FSqlType.MySql, false, "0")]
        [DataRow(FSqlType.SQLite, true, "1")]
        [DataRow(FSqlType.SQLite, false, "0")]
        [DataRow(FSqlType.PostgreSql, true, "true")]
        [DataRow(FSqlType.PostgreSql, false, "false")]
        [DataRow(FSqlType.DuckDB, true, "true")]
        [DataRow(FSqlType.DuckDB, false, "false")]
        public void WrapValueBoolean(FSqlType sqlType, bool val, string act)
        {
            var escaper = GetEscaper(sqlType);
            Assert.AreEqual(act, escaper!.WrapValue(val));
        }
    }
}
