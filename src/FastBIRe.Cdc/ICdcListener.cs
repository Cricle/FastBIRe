﻿using FastBIRe.Cdc.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc
{
    public interface ICdcListener : IDisposable
    {
        event EventHandler<CdcEventArgs>? EventRaised;

        event EventHandler<CdcErrorEventArgs>? Error;

        IGetCdcListenerOptions Options { get; }

        bool IsStarted { get; }

        Task StopAsync(CancellationToken token = default);

        Task StartAsync(CancellationToken token = default);

        ITableMapInfo? GetTableMapInfo(object id);
    }
}
