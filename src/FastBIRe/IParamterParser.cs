namespace FastBIRe
{
    public interface IParamterParser
    {
        IEnumerable<KeyValuePair<string, object?>> Parse(object? value);
    }
}
