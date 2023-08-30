using FastBIRe.Project.Accesstor;
using FastBIRe.Project.Models;
using System.Data;

namespace FastBIRe.Project.DynamicTable
{
    public record class DefaultDynamicColumn(string Id, string Name, DynamicTypes Type,bool Nullable,string? IndexGroup=null,int IndexOrder=0);
    public record class DefaultDynamicTable<TColumn>(string Id, string Name, List<TColumn> Columns)
        where TColumn : DefaultDynamicColumn
    {
        public TColumn? FindColumnById(string id)
        {
            return Columns?.FirstOrDefault(c => c.Id == id);
        }
        public TColumn? FindColumnByName(string name)
        {
            return Columns?.FirstOrDefault(c => c.Name == name);
        }
    }
    public record class DynamicProject<TId, TTable, TColumn>(TId Id, string Name, Version Version, DateTime CreateTime, List<TTable> Tables)
        : Project<TId>(Id, Name, Version, CreateTime)
        where TTable : DefaultDynamicTable<TColumn>
        where TColumn : DefaultDynamicColumn
    {
        public TTable? FindTable(string name)
        {
            return Tables?.FirstOrDefault(x => x.Name == name);
        }
    }
}
