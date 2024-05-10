﻿using System.Threading.Channels;

namespace Diagnostics.Traces
{
    public class BufferOperator<T> : IDisposable
    {
        private int disposeCount;
        private readonly Channel<T> channel;
        private readonly Task task;
        private readonly CancellationTokenSource tokenSource;

        public BufferOperator(IInputHandler<T> handler)
            : this(handler, true, false)
        {
        }
        public BufferOperator(IInputHandler<T> handler, bool wait, bool continueCaptureContext)
        {
            Wait = wait;
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            ContinueCaptureContext = continueCaptureContext;
            tokenSource = new CancellationTokenSource();
            channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions { SingleReader = true });
            task = Task.Factory.StartNew(HandlerAsync, this, TaskCreationOptions.LongRunning).Unwrap();
            Handler = handler;
        }

        public bool Wait { get; }

        public bool ContinueCaptureContext { get; }

        public IInputHandler<T> Handler { get; }

        public Task Task => task;

        public ChannelReader<T> Reader => channel.Reader;

        public event EventHandler<BufferOperatorExceptionEventArgs<T>>? ExceptionRaised;

        private async Task HandlerAsync(object? state)
        {
            var channel = (BufferOperator<T>)state!;
            var tokenSource = channel.tokenSource;
            var handler = channel.Handler;
            var wait = channel.Wait;
            var continueCaptureContext = channel.ContinueCaptureContext;
            T? t = default;
            while (!tokenSource.IsCancellationRequested)
            {
                try
                {
                    var args = await Reader.ReadAsync(tokenSource.Token);
                    var task = handler.HandleAsync(args, tokenSource.Token);
                    if (wait)
                    {
                        await task.ConfigureAwait(continueCaptureContext);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    ExceptionRaised?.Invoke(channel, new BufferOperatorExceptionEventArgs<T>(t, ex));
                }
                finally
                {
                    t = default;
                }
            }
            tokenSource.Dispose();
        }

        public void Add(T input)
        {
            var task = channel.Writer.WriteAsync(input);
            if (!task.IsCompleted)
            {
                task.AsTask().GetAwaiter().GetResult();
            }
        }

        public void Dispose()
        {
            if (Interlocked.Increment(ref disposeCount) > 1)
            {
                return;
            }

            tokenSource.Cancel();
            channel.Writer.Complete();
        }
    }
}