using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;
using System;
#if NETSTANDARD2_0
using System.Runtime.CompilerServices;
using System.Collections.Generic;
#endif

namespace Diagnostics.Generator.Core
{
    public abstract class SynchronousExecuter<T> : IDisposable
    {
        private readonly Channel<T> channel;

        public Task Task { get; }

        public CancellationTokenSource TokenSource { get; }

        public event EventHandler<CalculatorErrorEventArgs<T>>? ErrorRaised;

        public ChannelWriter<T> ChannelWriter { get; }

        public SynchronousExecuter()
        {
            TokenSource = new CancellationTokenSource();
            channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
            {
                SingleReader = true
            });
            ChannelWriter = channel.Writer;
            Task = Task.Factory.StartNew(ProcessAsync, TaskCreationOptions.LongRunning).Unwrap();
        }

        public void Add(T value)
        {
            channel.Writer.TryWrite(value);
        }

        public Task ComplatedAsync()
        {
            ChannelWriter.TryComplete();
            return Task;
        }

        private async Task ProcessAsync()
        {
            var tk = TokenSource.Token;
            var that = this;
#if NETSTANDARD2_0
            await foreach (var value in ReadAllAsync(channel.Reader, tk))
#else
            await foreach (var value in channel.Reader.ReadAllAsync(tk))
#endif
            {
                try
                {
                    var task = OnProcessAsync(value, tk);
                    if (!task.IsCompleted)
                    {
                        await task;
                    }
                    else if (task.Exception != null)
                    {
                        ErrorRaised?.Invoke(that, new CalculatorErrorEventArgs<T>(value, task.Exception));
                    }
                }
                catch (Exception ex)
                {
                    ErrorRaised?.Invoke(that, new CalculatorErrorEventArgs<T>(value, ex));
                }
            }
        }
#if NETSTANDARD2_0
        public virtual async IAsyncEnumerable<T> ReadAllAsync(ChannelReader<T> reader,[EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (reader.TryRead(out T? item))
                {
                    yield return item;
                }
            }
        }
#endif
        protected abstract Task OnProcessAsync(T value, CancellationToken token);

        public void Dispose()
        {
            TokenSource.Cancel();
            OnDisposed();
        }

        protected virtual void OnDisposed()
        {

        }
    }

}
