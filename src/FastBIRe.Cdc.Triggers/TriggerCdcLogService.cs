namespace FastBIRe.Cdc.Triggers
{
    public class TriggerCdcLogService : ICdcLogService
    {
        public static readonly TriggerCdcLogService Instance = new TriggerCdcLogService();

        private TriggerCdcLogService() { }

        public Task<IList<ICdcLog>> GetAllAsync(CancellationToken token = default)
        {
            return Task.FromResult<IList<ICdcLog>>(Array.Empty<CdcLog>());
        }

        public Task<ICdcLog?> GetLastAsync(CancellationToken token = default)
        {
            return Task.FromResult<ICdcLog?>(null);
        }
    }
}
