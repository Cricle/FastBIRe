namespace FastBIRe.Wrapping
{
    public interface IEscaper
    {
        string Quto<T>(T? input);

        string? WrapValue<T>(T? input);

        string? ReplaceParamterPrefixSql(string? sql, char originPrefix);

        string? ReplaceQutoSql(string? sql, char qutoStart,char qutoEnd);

    }
}
