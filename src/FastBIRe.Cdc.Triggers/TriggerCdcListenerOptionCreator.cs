using FastBIRe.Cdc.Mssql;

namespace FastBIRe.Cdc.Triggers
{
    public class TriggerCdcListenerOptionCreator : ICdcListenerOptionCreator
    {
        public TriggerCdcListenerOptionCreator(TimeSpan delayTime, uint readBatch)
        {
            DelayTime = delayTime;
            ReadBatch = readBatch;
        }

        public TimeSpan DelayTime { get; }

        public uint ReadBatch { get; }

        public Task<ICdcListener> CreateCdcListnerAsync(in CdcListenerOptionCreateInfo info, CancellationToken token = default)
        {
            return info.Runner.CdcManager.GetCdcListenerAsync(new TriggerGetCdcListenerOptions(info.Runner.SourceScriptExecuter,
                DelayTime,
                ReadBatch,
                info.TableNames,
                info.CheckPoint), token);
        }
    }
}
