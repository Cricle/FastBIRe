using System;

namespace FastBIRe.Cdc.Events
{
    public class CdcErrorEventArgs : EventArgs
    {
        public CdcErrorEventArgs(Exception? exception)
        {
            Exception = exception;
        }

        public Exception? Exception { get; }
    }
}
