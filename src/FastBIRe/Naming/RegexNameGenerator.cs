﻿using System.Text.RegularExpressions;

namespace FastBIRe.Naming
{
    public class RegexNameGenerator : INameGenerator
    {
        public const string BraceRegexExp = "({.*?})";

        public static readonly Regex BraceRegex = new Regex(BraceRegexExp, RegexOptions.Compiled);

        public RegexNameGenerator(string format)
        {
            ParseRegex = BraceRegex;
            Format = format;
            var braceCount = Count(format);
            MaxArgCount = braceCount;
            MinArgCount = braceCount;
        }
        public RegexNameGenerator(Regex parseRegex, string format, int maxArgCount, int minArgCount)
        {
            ParseRegex = parseRegex;
            Format = format;
            MaxArgCount = maxArgCount;
            MinArgCount = minArgCount;
        }

        public Regex ParseRegex { get; }

        public string Format { get; }

        public int MaxArgCount { get; }

        public int MinArgCount { get; }

        public int Count(string input)
        {
#if NET7_0_OR_GREATER
            return ParseRegex.Count(input);
#else
            return ParseRegex.Matches(input).Count;
#endif
        }

        public string Create(IEnumerable<object> args)
        {
            return string.Format(Format, args.ToArray());
        }

        public bool TryParse(string input, out IReadOnlyList<string>? results)
        {
            var matchs = ParseRegex.Matches(input);
            results = matchs.OfType<Capture>().Select(x => x.Value).ToList();
            return true;
        }
    }
}