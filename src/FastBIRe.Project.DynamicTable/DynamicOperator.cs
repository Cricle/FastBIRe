using FastBIRe.Project.Accesstor;
using System.Data;

namespace FastBIRe.Project.DynamicTable
{
    public class DynamicOperator<TResult, TInput, TProject, TId, TTable, TColumn> : IDisposable
        where TProject : DynamicProject<TId,TTable,TColumn>
        where TResult : ProjectCreateContextResult<TProject, TId>
        where TTable : DefaultDynamicTable<TColumn>
        where TColumn : DefaultDynamicColumn
        where TInput : IProjectAccesstContext<TId>
    {
        public DynamicOperator(ITableFactory<TResult, TProject, TId> tableFactory, IProjectAccesstor<TInput, TProject, TId> accesstor)
        {
            TableFactory = tableFactory ?? throw new ArgumentNullException(nameof(tableFactory));
            Accesstor = accesstor ?? throw new ArgumentNullException(nameof(accesstor));
        }

        public ITableFactory<TResult, TProject, TId> TableFactory { get; }

        public IProjectAccesstor<TInput,TProject,TId> Accesstor { get; }

        public IEnumerable<KeyValuePair<string, string?>> CaseValues<T>(TProject project, string name, IEnumerable<KeyValuePair<string, T>> inputs, bool notFoundThrow = false)
        {
            var table = project.Tables.FirstOrDefault(x => x.Name == name) ?? throw new InvalidOperationException($"Table {name} not found!");
            foreach (var item in inputs)
            {
                if (item.Value == null)
                {
                    yield return new KeyValuePair<string, string?>(item.Key, "NULL");
                }
                var col = table.Columns.FirstOrDefault(x => x.Name == item.Key);
                if (col != null)
                {
                    yield return new KeyValuePair<string, string?>(item.Key, AsValue(project, item.Key, item.Value,col, inputs));
                }
                else if (notFoundThrow)
                {
                    throw new InvalidOperationException($"Column {item.Key} not found in table {name}");
                }
            }
        }
        protected virtual string? AsValue<T>(TProject project, string columName, T value,TColumn column, IEnumerable<KeyValuePair<string, T>> inputs)
        {
            switch (column.Type)
            {
                case DynamicTypes.DateTime:
                case DynamicTypes.Text:
                    return $"'{value}'";
                case DynamicTypes.Number:
                    return value!.ToString();
                default:
                    throw new NotSupportedException(column.Type.ToString());
            }
        }

        public async Task<bool> DropAsync(TInput context, TProject project,string name,CancellationToken token=default)
        {
            var @class = project.Tables.FirstOrDefault(x => x.Name == name);
            if (@class != null)
            {
                var sql = TableFactory.Service.TableHelper.CreateDropTable(name);
                await TableFactory.Service.ExecuteNonQueryAsync(sql,token: token);
                project.Tables.Remove(@class);
                var tb = await Accesstor.UpdateProjectAsync(context, project);
                return true;
            }
            return false;
        }
        protected virtual Task OnUpdatingTableAsync(TInput context, TProject project, TTable table, CancellationToken token = default)
        {
            var @class = project.Tables.FirstOrDefault(x => x.Name == table.Name);
            if (@class == null)
            {
                project.Tables.Add(table);
            }
            else
            {
                var idx = project.Tables.IndexOf(@class);
                project.Tables.Remove(table);
                project.Tables.Insert(idx, table);
            }
            return Task.CompletedTask;
        }
        protected virtual Task OnUpsetedAsync(TInput context, TProject project, TTable table,bool succeed, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }
        protected virtual TableColumnDefine MakeColumn(TInput context, TProject project, TTable table, TColumn column,SourceTableColumnBuilder builder, CancellationToken token = default)
        {
            switch (column.Type)
            {
                case DynamicTypes.Text:
                    return builder.Column(column.Name, builder.Type(DbType.String, builder.StringLen), destNullable: column.Nullable, id: column.Id,indexGroup:column.IndexGroup,indexOrder:column.IndexOrder);
                case DynamicTypes.Number:
                    return builder.Column(column.Name, builder.Type(DbType.Decimal, builder.Scale, builder.Precision), destNullable: column.Nullable, id: column.Id, indexGroup: column.IndexGroup, indexOrder: column.IndexOrder);
                case DynamicTypes.DateTime:
                    return builder.Column(column.Name, builder.Type(DbType.DateTime), destNullable: column.Nullable, id: column.Id, indexGroup: column.IndexGroup, indexOrder: column.IndexOrder);
                default:
                    throw new NotSupportedException(column.Type.ToString());
            }
        }
        protected virtual Task OnComplatedMakeColumnAsync(TInput context, TProject project, TTable table, List<TableColumnDefine> columns, SourceTableColumnBuilder builder, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }
        public async Task<bool> UpsetAsync(TInput context,TProject project,TTable table,CancellationToken token=default)
        {
            var builder = TableFactory.Service.GetColumnBuilder();
            var columns = new List<TableColumnDefine>();
            foreach (var item in table.Columns)
            {
                columns.Add(MakeColumn(context, project, table, item, builder, token));
            }
            await OnComplatedMakeColumnAsync(context, project, table, columns, builder, token);
            var colsMerge = TableFactory.TableIniter.WithColumns(builder, columns);
            var result = await TableFactory.MigrateToSqlAsync(table.Name, colsMerge, null, token);
            await result.ExecuteAsync(token);            
            await OnUpdatingTableAsync(context, project, table, token);
            var res = await Accesstor.UpdateProjectAsync(context, project);
            await OnUpsetedAsync(context, project, table, res, token);
            return res;
        }

        public void Dispose()
        {
            TableFactory.Dispose();
        }
    }
}
