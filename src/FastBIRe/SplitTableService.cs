using Ao.Stock.Querying;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using System.Text;

namespace FastBIRe
{
    public class SplitTableService
    {
        public SplitTableService(SqlType sqlType, ISpliteStrategy spliteStrategy,IReadOnlyList<string> columnNames, IReadOnlyList<int> strategyIndexs)
            :this(sqlType,MergeHelper.GetMethodWrapper(sqlType), spliteStrategy,columnNames, strategyIndexs)
        {
        }
        public SplitTableService(SqlType sqlType, IMethodWrapper methodWrapper, ISpliteStrategy spliteStrategy, IReadOnlyList<string> columnNames, IReadOnlyList<int> strategyIndexs)
        {
            SqlType = sqlType;
            MethodWrapper = methodWrapper;
            SpliteStrategy = spliteStrategy;
            ColumnNames = columnNames;
            columnNameJoin = string.Join(",", ColumnNames.Select(x => methodWrapper.Quto(x)));
            StrategyIndexs = strategyIndexs;
        }

        private readonly string columnNameJoin;

        public SqlType SqlType { get; }

        public IMethodWrapper MethodWrapper { get; }

        public ISpliteStrategy SpliteStrategy { get; }

        public IReadOnlyList<string> ColumnNames { get; }

        public IReadOnlyList<int> StrategyIndexs { get; }

        public string InsertSql(IEnumerable<IReadOnlyList<object>> values)
        {
            var sb = new StringBuilder();
            InsertSql(sb, values);
            return sb.ToString();
        }
        public void InsertSql(StringBuilder builder, IEnumerable<IReadOnlyList<object>> values)
        {
            foreach (var item in values)
            {
                InsertSql(builder, item, 0);
                builder.AppendLine();
            }
        }
        private IEnumerable<object> GetStrategyValues(IReadOnlyList<object> values,int offset)
        {
            foreach (var item in StrategyIndexs)
            {
                yield return values[offset + item];
            }
        }

        public void InsertSql(StringBuilder builder,IReadOnlyList<object> values, int offset)
        {
            var table = SpliteStrategy.GetTable(GetStrategyValues(values,offset), offset);
            builder.Append("INSERT INTO ");
            builder.Append(MethodWrapper.Quto(table));
            builder.Append('(');
            builder.Append(columnNameJoin);
            builder.Append(") VALUES(");
            foreach (var item in values.Skip(offset).Take(ColumnNames.Count))
            {
                builder.Append(MethodWrapper.WrapValue(item));
                builder.Append(',');
            }
            builder.Remove(builder.Length - 2, 1);
            builder.Append(");");
        }
        public string InsertSql(IReadOnlyList<object> values,int offset)
        {
            var sb = new StringBuilder();
            InsertSql(sb, values, offset);
            return sb.ToString();
        }
    }
}
