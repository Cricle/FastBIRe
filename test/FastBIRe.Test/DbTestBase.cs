namespace FastBIRe.Test
{
    public abstract class DbTestBase
    {
        public string Quto(SqlType type,string name)
        {
            return type.Wrap(name);
        }

        protected readonly DatabaseIniter databaseIniter= DatabaseIniter.Instance;

    }
}
