namespace FastBIRe.ModelGen.Internal
{
    internal readonly struct WriteReadColumnResult
    {
        public WriteReadColumnResult(string oridinalCall, string writeCall)
        {
            OridinalCall = oridinalCall;
            WriteCall = writeCall;
        }

        public string OridinalCall { get; }

        public string WriteCall { get; }
    }
}
