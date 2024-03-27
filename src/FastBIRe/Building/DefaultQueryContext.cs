namespace FastBIRe.Building
{
    public class DefaultQueryContext
    {
        public string? Expression { get; set; }

        public bool MustQuto { get; set; }
    }
    public class SqlQueryContext : DefaultQueryContext
    {

    }
}
