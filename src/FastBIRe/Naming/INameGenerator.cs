namespace FastBIRe.Naming
{
    public interface INameGenerator
    {
        int MaxArgCount { get; }

        int MinArgCount { get; }

        string Create(IEnumerable<object> args);

        int Count(string input);

        bool TryParse(string input, out IReadOnlyList<string>? results);
    }
}
