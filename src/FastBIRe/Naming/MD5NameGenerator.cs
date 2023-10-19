namespace FastBIRe.Naming
{
    public class DefaultEffectTableKeyNameGenerator : INameGenerator
    {
        public static readonly DefaultEffectTableKeyNameGenerator Instance = new DefaultEffectTableKeyNameGenerator();

        private DefaultEffectTableKeyNameGenerator() { }

        public int MaxArgCount => 1;

        public int MinArgCount => 1;

        public int Count(string input)
        {
            return 1;
        }

        public string Create(IEnumerable<object> args)
        {
            var pair = string.Concat(args.Skip(1));
            return $"K_{args.First()}_{pair}";
        }

        public bool TryParse(string input, out IReadOnlyList<string>? results)
        {
            results = null;
            return false;
        }
    }
}
