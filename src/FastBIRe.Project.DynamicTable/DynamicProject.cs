using FastBIRe.Project.Models;

namespace FastBIRe.Project.DynamicTable
{
    public record class DefaultDynamicColumn
    {
        public DefaultDynamicColumn()
        {
        }

        public DefaultDynamicColumn(string id, string name, DynamicTypes type, bool nullable)
        {
            Id = id;
            Name = name;
            Type = type;
            Nullable = nullable;
        }

        public string? Id { get; set; }

        public string? Name { get; set; }

        public DynamicTypes Type { get; set; }

        public bool Nullable { get; set; }

        public string? IndexGroup { get; set; }

        public int IndexOrder { get; set; }
    }
    public record class DefaultDynamicTable<TColumn>
        where TColumn : DefaultDynamicColumn
    {
        public DefaultDynamicTable()
        {
        }

        public DefaultDynamicTable(string id, string name, List<TColumn> columns)
        {
            Id = id;
            Name = name;
            Columns = columns;
        }

        public string? Id { get; set; }

        public string? Name { get; set; }

        public List<TColumn>? Columns { get; set; }

        public TColumn? FindColumnById(string id)
        {
            return Columns?.FirstOrDefault(c => c.Id == id);
        }
        public TColumn? FindColumnByName(string name)
        {
            return Columns?.FirstOrDefault(c => c.Name == name);
        }
    }
    public record class DynamicProject<TId, TTable, TColumn>
        : Project<TId>
        where TTable : DefaultDynamicTable<TColumn>
        where TColumn : DefaultDynamicColumn
    {
        public DynamicProject()
        {

        }

        public DynamicProject(TId id, string name, Version version, DateTime createTime, List<TTable> tables) 
            : base(id, name, version, createTime)
        {
            Tables = tables;   
        }
        public List<TTable>? Tables { get; set; }

        public TTable? FindTable(string name)
        {
            return Tables?.FirstOrDefault(x => x.Name == name);
        }
    }
}
