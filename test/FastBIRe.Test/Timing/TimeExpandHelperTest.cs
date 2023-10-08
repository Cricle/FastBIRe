using FastBIRe.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBIRe.Test.Timing
{
    [TestClass]
    public class TimeExpandHelperTest
    {
        [TestMethod]
        [DataRow(SqlType.MySql, "__$field_second", "__$field_second")]
        [DataRow(SqlType.SqlServer, "__$field_second", "__$field_second")]
        [DataRow(SqlType.SqlServerCe, "__$field_second", "__$field_second")]
        [DataRow(SqlType.SQLite, "__$field_second", "__$field_second")]
        [DataRow(SqlType.PostgreSql, "__$field_second", "__$field_second")]
        public void Create_Second(SqlType sqlType,string name,string formatExp)
        {
            var helper = new TimeExpandHelper(sqlType);
            var res = helper.Create("field", TimeTypes.Second).ToList();
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("field", res[0].OriginName);
            Assert.AreEqual(TimeTypes.Second, res[0].Type);
            Assert.AreEqual(name, res[0].Name);
            Assert.AreEqual(formatExp, res[0].ExparessionFormatter);
        }
        [TestMethod]
        [DataRow(SqlType.MySql, "__$field_minute", "CONCAT(CAST(LEFT({0},16) AS CHAR) , CAST(':00' AS CHAR))")]
        [DataRow(SqlType.SqlServer, "__$field_minute", "CONCAT(CONVERT(VARCHAR,CONVERT(VARCHAR(16),{0} ,120),120) , CONVERT(VARCHAR,':00',120))")]
        [DataRow(SqlType.SqlServerCe, "__$field_minute", "CONCAT(CONVERT(VARCHAR,CONVERT(VARCHAR(16),{0} ,120),120) , CONVERT(VARCHAR,':00',120))")]
        [DataRow(SqlType.SQLite, "__$field_minute", "CAST(strftime('%Y-%m-%d %H:%M', {0}) AS TEXT) || CAST(':00' AS TEXT)")]
        [DataRow(SqlType.PostgreSql, "__$field_minute", "LEFT(date_trunc('minute',{0})::VARCHAR,16)::VARCHAR || ':00'::VARCHAR")]
        public void Create_Minute(SqlType sqlType, string name, string formatExp)
        {
            var helper = new TimeExpandHelper(sqlType);
            var res = helper.Create("field", TimeTypes.Minute).ToList();
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("field", res[0].OriginName);
            Assert.AreEqual(TimeTypes.Minute, res[0].Type);
            Assert.AreEqual(name, res[0].Name);
            Assert.AreEqual(formatExp, res[0].ExparessionFormatter!.Trim());
        }
        [TestMethod]
        [DataRow(SqlType.MySql, "__$field_hour", "CONCAT(CAST(LEFT({0},13) AS CHAR) , CAST(':00:00' AS CHAR))")]
        [DataRow(SqlType.SqlServer, "__$field_hour", "CONCAT(CONVERT(VARCHAR,CONVERT(VARCHAR(13),{0} ,120),120) , CONVERT(VARCHAR,':00:00',120))")]
        [DataRow(SqlType.SqlServerCe, "__$field_hour", "CONCAT(CONVERT(VARCHAR,CONVERT(VARCHAR(13),{0} ,120),120) , CONVERT(VARCHAR,':00:00',120))")]
        [DataRow(SqlType.SQLite, "__$field_hour", "CAST(strftime('%Y-%m-%d %H', {0}) AS TEXT) || CAST(':00:00' AS TEXT)")]
        [DataRow(SqlType.PostgreSql, "__$field_hour", "LEFT(date_trunc('hour',{0})::VARCHAR,13)::VARCHAR || ':00:00'::VARCHAR")]
        public void Create_Hour(SqlType sqlType, string name, string formatExp)
        {
            var helper = new TimeExpandHelper(sqlType);
            var res = helper.Create("field", TimeTypes.Hour).ToList();
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("field", res[0].OriginName);
            Assert.AreEqual(TimeTypes.Hour, res[0].Type);
            Assert.AreEqual(name, res[0].Name);
            Assert.AreEqual(formatExp, res[0].ExparessionFormatter!.Trim());
        }
        [TestMethod]
        [DataRow(SqlType.MySql, "__$field_day", "CONCAT(CAST(LEFT({0},10) AS CHAR) , CAST(' 00:00:00' AS CHAR))")]
        [DataRow(SqlType.SqlServer, "__$field_day", "CONCAT(CONVERT(VARCHAR,CONVERT(VARCHAR(10),{0} ,120),120) , CONVERT(VARCHAR,' 00:00:00',120))")]
        [DataRow(SqlType.SqlServerCe, "__$field_day", "CONCAT(CONVERT(VARCHAR,CONVERT(VARCHAR(10),{0} ,120),120) , CONVERT(VARCHAR,' 00:00:00',120))")]
        [DataRow(SqlType.SQLite, "__$field_day", "CAST(strftime('%Y-%m-%d', {0}) AS TEXT) || CAST(' 00:00:00' AS TEXT)")]
        [DataRow(SqlType.PostgreSql, "__$field_day", "LEFT(date_trunc('day',{0})::VARCHAR,10)::VARCHAR || ' 00:00:00'::VARCHAR")]
        public void Create_Day(SqlType sqlType, string name, string formatExp)
        {
            var helper = new TimeExpandHelper(sqlType);
            var res = helper.Create("field", TimeTypes.Day).ToList();
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("field", res[0].OriginName);
            Assert.AreEqual(TimeTypes.Day, res[0].Type);
            Assert.AreEqual(name, res[0].Name);
            Assert.AreEqual(formatExp, res[0].ExparessionFormatter!.Trim());
        }
        [TestMethod]
        [DataRow(SqlType.MySql, "__$field_week", "DATE_FORMAT(DATE_SUB({0}, INTERVAL WEEKDAY({0}) DAY),'%Y-%m-%d')")]
        [DataRow(SqlType.SqlServer, "__$field_week", "DATEADD(WEEK, DATEDIFF(WEEK, 0, CONVERT(DATETIME, {0}, 120) - 1), 0)")]
        [DataRow(SqlType.SqlServerCe, "__$field_week", "DATEADD(WEEK, DATEDIFF(WEEK, 0, CONVERT(DATETIME, {0}, 120) - 1), 0)")]
        [DataRow(SqlType.SQLite, "__$field_week", "date({0}, 'weekday 0', '-6 day')||' 00:00:00'")]
        [DataRow(SqlType.PostgreSql, "__$field_week", "(date_trunc('day',{0}) - ((EXTRACT(DOW FROM {0})::INTEGER+6)%7 || ' days')::INTERVAL)::timestamp with time zone")]
        public void Create_Week(SqlType sqlType, string name, string formatExp)
        {
            var helper = new TimeExpandHelper(sqlType);
            var res = helper.Create("field", TimeTypes.Week).ToList();
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("field", res[0].OriginName);
            Assert.AreEqual(TimeTypes.Week, res[0].Type);
            Assert.AreEqual(name, res[0].Name);
            Assert.AreEqual(formatExp, res[0].ExparessionFormatter!.Trim());
        }
        [TestMethod]
        [DataRow(SqlType.MySql, "__$field_month", "CONCAT(CAST(LEFT({0},7) AS CHAR) , CAST('-01 00:00:00' AS CHAR))")]
        [DataRow(SqlType.SqlServer, "__$field_month", "CONCAT(CONVERT(VARCHAR,CONVERT(VARCHAR(7),{0} ,120),120) , CONVERT(VARCHAR,'-01 00:00:00',120))")]
        [DataRow(SqlType.SqlServerCe, "__$field_month", "CONCAT(CONVERT(VARCHAR,CONVERT(VARCHAR(7),{0} ,120),120) , CONVERT(VARCHAR,'-01 00:00:00',120))")]
        [DataRow(SqlType.SQLite, "__$field_month", "CAST(strftime('%Y-%m', {0}) AS TEXT) || CAST('-01 00:00:00' AS TEXT)")]
        [DataRow(SqlType.PostgreSql, "__$field_month", "LEFT(date_trunc('month',{0})::VARCHAR,7)::VARCHAR || '-01 00:00:00'::VARCHAR")]
        public void Create_Month(SqlType sqlType, string name, string formatExp)
        {
            var helper = new TimeExpandHelper(sqlType);
            var res = helper.Create("field", TimeTypes.Month).ToList();
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("field", res[0].OriginName);
            Assert.AreEqual(TimeTypes.Month, res[0].Type);
            Assert.AreEqual(name, res[0].Name);
            Assert.AreEqual(formatExp, res[0].ExparessionFormatter!.Trim());
        }
        [TestMethod]
        [DataRow(SqlType.MySql, "__$field_quarter", "CONCAT(DATE_FORMAT(LAST_DAY(MAKEDATE(EXTRACT(YEAR FROM {0}),1) + interval QUARTER({0})*3-3 month),'%Y-%m-'),'01')")]
        [DataRow(SqlType.SqlServer, "__$field_quarter", "DATEADD(qq, DATEDIFF(qq, 0, {0}), 0)")]
        [DataRow(SqlType.SqlServerCe, "__$field_quarter", "DATEADD(qq, DATEDIFF(qq, 0, {0}), 0)")]
        [DataRow(SqlType.SQLite, "__$field_quarter", "STRFTIME('%Y', {0})||'-'||(CASE \r\n        WHEN COALESCE(NULLIF((SUBSTR({0}, 4, 2) - 1) / 3, 0), 4) < 10 \r\n        THEN '0' || COALESCE(NULLIF((SUBSTR({0}, 4, 2) - 1) / 3, 0), 4)\r\n        ELSE COALESCE(NULLIF((SUBSTR({0}, 4, 2) - 1) / 3, 0), 4)\r\n    END)||'-01 00:00:00'")]
        [DataRow(SqlType.PostgreSql, "__$field_quarter", "date_trunc('quarter', {0}::TIMESTAMP)")]
        public void Create_Quarter(SqlType sqlType, string name, string formatExp)
        {
            var helper = new TimeExpandHelper(sqlType);
            var res = helper.Create("field", TimeTypes.Quarter).ToList();
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("field", res[0].OriginName);
            Assert.AreEqual(TimeTypes.Quarter, res[0].Type);
            Assert.AreEqual(name, res[0].Name);
            Assert.AreEqual(formatExp, res[0].ExparessionFormatter!.Trim());
        }
        [TestMethod]
        [DataRow(SqlType.MySql, "__$field_year", "CONCAT(CAST(LEFT({0},4) AS CHAR) , CAST('-01-01 00:00:00' AS CHAR))")]
        [DataRow(SqlType.SqlServer, "__$field_year", "CONCAT(CONVERT(VARCHAR,CONVERT(VARCHAR(4),{0} ,120),120) , CONVERT(VARCHAR,'-01-01 00:00:00',120))")]
        [DataRow(SqlType.SqlServerCe, "__$field_year", "CONCAT(CONVERT(VARCHAR,CONVERT(VARCHAR(4),{0} ,120),120) , CONVERT(VARCHAR,'-01-01 00:00:00',120))")]
        [DataRow(SqlType.SQLite, "__$field_year", "CAST(strftime('%Y', {0}) AS TEXT) || CAST('-01-01 00:00:00' AS TEXT)")]
        [DataRow(SqlType.PostgreSql, "__$field_year", "LEFT(date_trunc('year',{0})::VARCHAR,4)::VARCHAR || '-01-01 00:00:00'::VARCHAR")]
        public void Create_Year(SqlType sqlType, string name, string formatExp)
        {
            var helper = new TimeExpandHelper(sqlType);
            var res = helper.Create("field", TimeTypes.Year).ToList();
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("field", res[0].OriginName);
            Assert.AreEqual(TimeTypes.Year, res[0].Type);
            Assert.AreEqual(name, res[0].Name);
            Assert.AreEqual(formatExp, res[0].ExparessionFormatter!.Trim());
        }
        [TestMethod]
        public void Throws()
        {
            Assert.ThrowsException<NotSupportedException>(() => new TimeExpandHelper(SqlType.Db2));
            var nameGen = TimeExpandHelper.DefaultNameGenerator;
            var timeNameMapper = TimeNameMapper.Instance;
            var funMapper = new FunctionMapper(SqlType.MySql);
            Assert.ThrowsException<ArgumentNullException>(() => new TimeExpandHelper(null!, timeNameMapper, funMapper));
            Assert.ThrowsException<ArgumentNullException>(() => new TimeExpandHelper(nameGen, null!, funMapper));
            Assert.ThrowsException<ArgumentNullException>(() => new TimeExpandHelper(nameGen, timeNameMapper, null!));
        }
    }
}
