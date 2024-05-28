using FastBIRe.Cdc.Checkpoints;
using System.Threading.Tasks;

namespace FastBIRe.Cdc
{
    public static class CdcManagerGetExtensions
    {
        public static async Task<ICheckpoint?> CreateCheckpointAsync(this ICdcManager cdcManager, byte[] data)
        {
            var mgr = await cdcManager.GetCdcCheckPointManagerAsync();
            return mgr.CreateCheckpoint(data);
        }
    }
}
