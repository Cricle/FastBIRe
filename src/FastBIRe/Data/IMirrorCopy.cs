namespace FastBIRe.Data
{
    public interface IMirrorCopy<TResult>
    {
        Task<IList<TResult>> CopyAsync(CancellationToken token = default);
    }
}
