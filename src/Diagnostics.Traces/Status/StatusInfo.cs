namespace Diagnostics.Traces.Status
{
    public readonly record struct StatusInfo
    {
        public StatusInfo(DateTime time, string? nowStatus, IReadOnlyList<TimePairValue> logs, IReadOnlyList<TimePairValue> status, DateTime? comapltedTime, StatusTypes? complatedStatus)
        {
            Time = time;
            NowStatus = nowStatus;
            Logs = logs;
            Status = status;
            ComapltedTime = comapltedTime;
            ComplatedStatus = complatedStatus;
        }

        public DateTime Time { get; }

        public string? NowStatus { get; }

        public IReadOnlyList<TimePairValue> Logs { get; }

        public IReadOnlyList<TimePairValue> Status { get; }

        public DateTime? ComapltedTime { get; }

        public StatusTypes? ComplatedStatus { get; }
    }
}