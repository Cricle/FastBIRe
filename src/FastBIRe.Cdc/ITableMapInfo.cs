namespace FastBIRe.Cdc
{
    public interface ITableMapInfo
    {
        object Id { get; }

        string DatabaseName { get; }

        string TableName { get; }
    }
}
