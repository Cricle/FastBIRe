namespace FastBIRe
{
    public interface IEntityColumnsSnapshot
    {
        Type EntityType { get; }

        IReadOnlyList<string> Keys { get; }

        IReadOnlyList<string> Indexs { get; }

        IReadOnlyList<string> Nullables { get; }

        IReadOnlyList<string> NotNulls { get; }

        IReadOnlyList<string> Columns { get; }
    }
}
