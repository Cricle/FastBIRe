namespace FastBIRe.Wrapping
{
    public interface IEscaper
    {
        string Quto<T>(T? input);

        string? WrapValue<T>(T? input);
    }

}
