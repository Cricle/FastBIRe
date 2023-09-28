﻿namespace FastBIRe
{
    public delegate Task ReadDataHandler(IScriptExecuter executer,ReadingDataArgs args);
    public interface IScriptExecuter
    {
        Task<int> ExecuteAsync(string script, CancellationToken token = default);

        Task<int> ExecuteAsync(IEnumerable<string> scripts, CancellationToken token = default);

        Task ReadAsync(string script, ReadDataHandler handler, CancellationToken token = default);
    }
}
