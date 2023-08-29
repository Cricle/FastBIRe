using FastBIRe.Project.Models;

namespace FastBIRe.Project.WebSample
{
	public enum FastDataType
	{
		Text,
		Number,
		DateTime
	}
	public record ClassColumn(string Id, string Name,bool Nullable, FastDataType Type);
	public record ClassTable(string Name,List<ClassColumn> Columns);
    public record SchoolProject(string Id, string Name, Version Version, DateTime CreateTime,List<ClassTable> Classes) : Project<string>(Id, Name, Version, CreateTime);
}
