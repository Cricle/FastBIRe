using System.Data;

namespace FastBIRe
{
    public readonly struct ReadingDataArgs
    {
        public ReadingDataArgs(string script, IDataReader reader, IQueryTranslateResult translateResult, CancellationToken token)
        {
            Script = script;
            Reader = reader;
            Token = token;
            TranslateResult = translateResult;
        }

        public string Script { get; }

        public IDataReader Reader { get; }

        public CancellationToken Token { get; }

        public IQueryTranslateResult TranslateResult { get; }
    }
}
