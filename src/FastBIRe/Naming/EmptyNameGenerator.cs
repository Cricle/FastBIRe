namespace FastBIRe.Naming
{
    public class EmptyNameGenerator : INameGenerator
    {
        public static readonly EmptyNameGenerator Instance = new EmptyNameGenerator();

        private EmptyNameGenerator() { }

        public int MaxArgCount => 0;

        public int MinArgCount => 0;

        public int Count(string input)
        {
            return 1;
        }

        public string Create(IEnumerable<object> args)
        {
            return string.Concat(args);
        }

        public bool TryParse(string input, out IReadOnlyList<string>? results)
        {
            results = new List<string>(1) { input };
            return true;
        }
    }
}
