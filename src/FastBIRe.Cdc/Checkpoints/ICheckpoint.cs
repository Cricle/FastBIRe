using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe.Cdc.Checkpoints
{
    public interface ICheckpoint
    {
        byte[] ToBytes();
    }
}
