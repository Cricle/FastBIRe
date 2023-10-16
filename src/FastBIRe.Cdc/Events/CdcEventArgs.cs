using System;

namespace FastBIRe.Cdc.Events
{
    public class CdcEventArgs : EventArgs
    {
        public CdcEventArgs(object? rawData)
        {
            RawData = rawData;
        }

        public object? RawData { get; }
    }
}
