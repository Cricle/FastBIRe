using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Diagnostics.Generator.Core
{
    public class BatchBufferOperator<T> : IDisposable
    {
        private readonly Channel<BatchData<T>> channel;
        private readonly Task task, taskTimeLoop;
        private readonly CancellationTokenSource tokenSource;
        private readonly object locker;
        private T[] currentBuffer = null!;
        private int bufferIndex;

        public BatchBufferOperator(IBatchOperatorHandler<T> handler, int bufferSize = 512, int swapDelayTimeMs = 5000)
        {
            BufferSize = bufferSize;
            locker = new object();
            channel = Channel.CreateUnbounded<BatchData<T>>();
            tokenSource = new CancellationTokenSource();
            Swap();
            Handler = handler;
            task = Task.Factory.StartNew(HandleAsync, this);
            taskTimeLoop = Task.Factory.StartNew(HandleTimeLoopAsync, this);
            SwapDelayTimeMs = swapDelayTimeMs;
        }

        public int BufferSize { get; }

        public int SwapDelayTimeMs { get; }

        public IBatchOperatorHandler<T> Handler { get; }

        public event EventHandler<Exception>? ExceptionRaised;

        private async Task HandleTimeLoopAsync(object? state)
        {
            var opetator = (BatchBufferOperator<T>)state!;
            var tk = opetator.tokenSource;
            var delayTime = opetator.SwapDelayTimeMs;

            while (!tk.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(delayTime, tk.Token);
                    Swap();
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    ExceptionRaised?.Invoke(this, ex);
                }
            }
        }

        private async Task HandleAsync(object? state)
        {
            var opetator = (BatchBufferOperator<T>)state!;
            var tk = opetator.tokenSource;
            var reader = opetator.channel.Reader;
            var handler = opetator.Handler;

            while (!tk.IsCancellationRequested)
            {
                try
                {
                    var res = await reader.ReadAsync();
                    try
                    {
                        await handler.HandleAsync(res, tk.Token);
                    }
                    finally
                    {
                        res.Dispose();
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    ExceptionRaised?.Invoke(this, ex);
                }
            }
            while (reader.Count != 0)
            {
                var res = await reader.ReadAsync();
                res.Dispose();
            }
            tk.Dispose();
        }
        public void Add(T t)
        {
            lock (locker)
            {
                UnsafeAdd(t);
            }
        }
        public void AddRange(ICollection<T> ts)
        {
            lock (locker)
            {
                if (bufferIndex + ts.Count >= bufferIndex)
                {
                    Swap();
                }
                CopyTo(currentBuffer.AsSpan(bufferIndex), ts);
                bufferIndex += ts.Count;
            }
        }
        private void UnsafeAdd(T t)
        {
            currentBuffer[bufferIndex++] = t;
            if (bufferIndex >= currentBuffer.Length)
            {
                Swap();
            }
        }
        private void CopyTo(Span<T> buffer, ICollection<T> ts)
        {
            if (ts is T[] array)
            {
                array.AsSpan().CopyTo(buffer);
                bufferIndex += ts.Count;
            }
#if NET8_0_OR_GREATER
            else if (ts is List<T> list)
            {
                CollectionsMarshal.AsSpan(list).CopyTo(buffer);
                bufferIndex += ts.Count;
            }
#endif
            else
            {
                foreach (var item in ts)
                {
                    UnsafeAdd(item);
                }
            }
        }
        private void Swap()
        {
            if (bufferIndex == 0 && currentBuffer != null)
            {
                return;
            }
            if (currentBuffer != null)
            {
                channel.Writer.TryWrite(new BatchData<T>(currentBuffer, bufferIndex));
            }
            currentBuffer = ArrayPool<T>.Shared.Rent(BufferSize);
            bufferIndex = 0;
        }

        public void Dispose()
        {
            channel.Writer.Complete();
            tokenSource.Cancel();
        }
    }
}
