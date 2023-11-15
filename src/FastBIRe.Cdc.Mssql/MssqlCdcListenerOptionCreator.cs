namespace FastBIRe.Cdc.Mssql
{
    public class MssqlCdcListenerOptionCreator : ICdcListenerOptionCreator
    {
        private static TimeSpan defaultDelayTime = TimeSpan.FromSeconds(5);

        public static TimeSpan DefaultDelayTime
        {
            get => defaultDelayTime;
            set
            {
                if (value.TotalMilliseconds <= 0)
                {
                    throw new ArgumentOutOfRangeException("The delay time must more than 1ms");
                }
                defaultDelayTime = value;
            }
        }
        public MssqlCdcListenerOptionCreator()
            : this(defaultDelayTime)
        {

        }

        public MssqlCdcListenerOptionCreator(TimeSpan delayTime)
        {
            DelayTime = delayTime;
        }

        public TimeSpan DelayTime { get; }

        public Task<ICdcListener> CreateCdcListnerAsync(in CdcListenerOptionCreateInfo info, CancellationToken token = default)
        {
            return info.Runner.CdcManager.GetCdcListenerAsync(new MssqlGetCdcListenerOptions(DelayTime, info.Runner.SourceScriptExecuter, info.TableNames, info.CheckPoint), token);
        }
    }
}
