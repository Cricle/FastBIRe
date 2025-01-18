namespace FastBIRe.Timescale
{
    public interface ITimescaleManager
    {
        string Table { get; }

        string TimeColumn { get; }

        string? ChuckTimeInterval { get; }

        int CommandTimeout { get; }

        Task<bool> IsHypertableAsync();

        Task<bool> CreateHypertableAsync(bool quto);
    }
}
