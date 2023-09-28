using System.Data;

namespace FastBIRe
{
    public readonly struct ReadingDataArgs
    {
        public ReadingDataArgs(string script, IDataReader reader, CancellationToken token)
        {
            Script = script;
            Reader = reader;
            Token = token;
        }

        public string Script { get; }

        public IDataReader Reader { get; }

        public CancellationToken Token { get; }
    }
}
