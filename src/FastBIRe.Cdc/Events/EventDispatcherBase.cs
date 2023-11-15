using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.Events
{
    public abstract class EventDispatcherBase<TInput> : IEventDispatcher<TInput>
    {
        public EventDispatcherBase()
            : this(true, null, false)
        {

        }

        public EventDispatcherBase(bool waitListener, TimeSpan? timeout, bool continueCaptureContext)
        {
            WaitListener = waitListener;
            Timeout = timeout;
            ContinueCaptureContext = continueCaptureContext;
        }

        private Task? task;
        private CancellationTokenSource? tokenSource;
        private int isStarted;

        public TimeSpan? Timeout { get; }

        public bool WaitListener { get; }

        public abstract int? Length { get; }

        public bool ContinueCaptureContext { get; }

        public Task? Task => task;

        public bool IsStarted => Volatile.Read(ref isStarted) != 0;

        protected abstract Task<TInput?> TryReadAsync(CancellationToken token = default);

        public abstract Task HandleAsync(TInput eventArgs, CancellationToken cancellationToken = default);

        protected virtual Task StartHandleAsync(TInput eventArgs, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        protected virtual Task EndHandleAsync(TInput eventArgs, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        private async Task HandlerAsync(object? state)
        {
            var channel = (EventDispatcherBase<TInput>)state!;
            var tokenSource = channel.tokenSource!;
            var waitListener = channel.WaitListener;
            var timeout = channel.Timeout;
            var continueCaptureContext = channel.ContinueCaptureContext;
            await TaskStartAsync();
            while (!tokenSource.IsCancellationRequested)
            {
                try
                {
                    while (true)
                    {
                        var args = await TryReadAsync(tokenSource.Token);
                        if (args != null)
                        {
                            var startTime = Stopwatch.StartNew();
                            var token = CancellationToken.None;
                            CancellationTokenSource? source = null;
                            if (timeout != null)
                            {
                                source = new CancellationTokenSource(timeout.Value);
                                token = source.Token;
                            }
                            await StartHandleAsync(args, tokenSource.Token);
                            try
                            {
                                var task = HandleAsync(args, token);
                                if (waitListener)
                                {
                                    await task.ConfigureAwait(continueCaptureContext);
                                }
                            }
                            catch (Exception ex)
                            {
                                HandleException(ex);
                            }
                            finally
                            {
                                await EndHandleAsync(args, tokenSource.Token);
                                source?.Dispose();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
            tokenSource.Dispose();
            await TaskEndAsync();
        }
        protected virtual Task TaskStartAsync()
        {
            return Task.CompletedTask;
        }
        protected virtual Task TaskEndAsync()
        {
            return Task.CompletedTask;
        }
        protected virtual void HandleException(Exception ex)
        {

        }

        public abstract void Add(TInput args);

        public virtual void AddRange(IEnumerable<TInput> args)
        {
            foreach (var item in args)
            {
                Add(item);
            }
        }

        public void Dispose()
        {
            tokenSource?.Cancel();
            OnDispose();
        }

        protected virtual void OnDispose()
        {

        }

        public Task StartAsync(CancellationToken token = default)
        {
            if (Interlocked.CompareExchange(ref isStarted, 1, 0) == 0)
            {
                tokenSource = new CancellationTokenSource();
                task = Task.Factory.StartNew(HandlerAsync, this, TaskCreationOptions.LongRunning).Unwrap();
            }
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken token = default)
        {
            if (Interlocked.CompareExchange(ref isStarted, 0, 1) == 1)
            {
                Debug.Assert(task != null);
                Debug.Assert(tokenSource != null);

                tokenSource!.Cancel();
                await task!;
                task = null;
                tokenSource = null;
            }
        }
    }
}
