namespace FastBIRe.Test
{
    public abstract class DbTestBase
    {
        public string Quto(FSqlType type, string name)
        {
            return type.Wrap(name);
        }

        protected readonly DatabaseIniter databaseIniter = DatabaseIniter.Instance;

    }
}
