using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.Events
{
    public class ChannelEventDispatcher<TInput> : EventDispatcherBase<TInput>
    {
        public ChannelEventDispatcher(bool waitListener, TimeSpan? timeout, bool continueCaptureContext, IEventDispatcheHandler<TInput> handler)
            : base(waitListener, timeout, continueCaptureContext)
        {
            channel = Channel.CreateUnbounded<TInput>(new UnboundedChannelOptions { SingleReader = true });
            Handler = handler;
        }

        public ChannelEventDispatcher(IEventDispatcheHandler<TInput> handler)
            : this(true, null, false, handler)
        {
        }

        private readonly Channel<TInput> channel;

        public IEventDispatcheHandler<TInput> Handler { get; }

        public override int? Length => channel.Reader.Count;

        public override void Add(TInput args)
        {
            channel.Writer.TryWrite(args);
        }

        public override Task HandleAsync(TInput eventArgs, CancellationToken cancellationToken = default)
        {
            return Handler.HandleAsync(eventArgs, cancellationToken);
        }

        protected override async Task<TInput?> TryReadAsync(CancellationToken token = default)
        {
            var item = await channel.Reader.ReadAsync(token);
            return item;
        }
    }
}
