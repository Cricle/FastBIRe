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
            results = null ;
            return false;
        }
    }
    public class MD5NameGenerator : INameGenerator
    {
        public static readonly MD5NameGenerator Instance = new MD5NameGenerator();

        private MD5NameGenerator() { }

        public int MaxArgCount => 1;

        public int MinArgCount => 1;

        public int Count(string input)
        {
            return 1;
        }

        public string Create(IEnumerable<object> args)
        {
            var res = string.Concat(args);
            return MD5Helper.ComputeHash(res);
        }

        public bool TryParse(string input, out IReadOnlyList<string>? results)
        {
            results = null;
            return false;
        }
    }
}
