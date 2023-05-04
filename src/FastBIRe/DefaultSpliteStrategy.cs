namespace FastBIRe
{
    public class DefaultSpliteStrategy : ISpliteStrategy
    {
        public DefaultSpliteStrategy(string initialName, ISpliteStrategyTablePartConverter tablePartConverter)
        {
            InitialName = initialName;
            TablePartConverter = tablePartConverter;
        }

        public string InitialName { get; }

        public ISpliteStrategyTablePartConverter TablePartConverter { get; }

        public string GetTable(IEnumerable<object> values, int offset)
        {
            return InitialName + string.Join("_", values.Select(TablePartConverter.Convert));
        }
    }
}
