using FastBIRe.Cdc.Mssql;

namespace FastBIRe.Cdc.Triggers
{
    public class TriggerCdcListenerOptionCreator : ICdcListenerOptionCreator
    {
        public TriggerCdcListenerOptionCreator(TimeSpan delayTime, uint readBatch, Func<CdcListenerOptionCreateInfo, IEnumerable<string>> tableNameGetter)
        {
            DelayTime = delayTime;
            ReadBatch = readBatch;
            TableNameGetter = tableNameGetter;
        }

        public TimeSpan DelayTime { get; }

        public uint ReadBatch { get; }

        public Func<CdcListenerOptionCreateInfo, IEnumerable<string>> TableNameGetter { get; }

        public Task<ICdcListener> CreateCdcListnerAsync(CdcListenerOptionCreateInfo info, CancellationToken token = default)
        {
            var tableNames= TableNameGetter(info);
            return info.Runner.CdcManager.GetCdcListenerAsync(new TriggerGetCdcListenerOptions(info.Runner.SourceScriptExecuter,
                DelayTime,
                ReadBatch,
                info.CheckPoint, 
                tableNames), token);
        }
    }
}
