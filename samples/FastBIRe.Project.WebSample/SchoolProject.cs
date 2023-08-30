using FastBIRe.Project.Accesstor;
using FastBIRe.Project.DynamicTable;

namespace FastBIRe.Project.WebSample
{
    public class SchoolDynamicOperator : DynamicOperator<ProjectCreateWithDbContextResult<SchoolProject, string>, IProjectAccesstContext<string>, SchoolProject, string, DefaultDynamicTable<DefaultDynamicColumn>, DefaultDynamicColumn>
    {
        public SchoolDynamicOperator(ITableFactory<ProjectCreateWithDbContextResult<SchoolProject, string>, SchoolProject, string> tableFactory, 
            IProjectAccesstor<IProjectAccesstContext<string>, SchoolProject, string> accesstor)
            : base(tableFactory, accesstor)
        {
        }
    }
    public record SchoolProject(string Id, string Name, Version Version, DateTime CreateTime, List<DefaultDynamicTable<DefaultDynamicColumn>> Tables)
        : DynamicProject<string,DefaultDynamicTable<DefaultDynamicColumn>, DefaultDynamicColumn>(Id,Name,Version,CreateTime,Tables);
}
