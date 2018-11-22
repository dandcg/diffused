using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Diffused.Core.Mediatr.Actor
{
    public class ActorManager
    {
        private readonly IMediator mediator;

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public ActorManager(IMediator mediator)
        {
            this.mediator = mediator;
        }

        internal ConcurrentDictionary<Type, Lazy<IMailbox>> Mailboxes { get; } = new ConcurrentDictionary<Type, Lazy<IMailbox>>();

        public Task<bool> Send<TRequest>(TRequest request)
        {
            var mailboxLazy = Mailboxes.GetOrAdd(typeof(TRequest), key => new Lazy<IMailbox>(() => new Mailbox<TRequest>(mediator, cts.Token)));

            var mailbox = (Mailbox<TRequest>) mailboxLazy.Value;

            return mailbox.Send(request);
        }

        public Task<bool> Post<TRequest>(TRequest request)
        {
            var mailboxLazy = Mailboxes.GetOrAdd(typeof(TRequest), key => new Lazy<IMailbox>(() => new Mailbox<TRequest>(mediator, cts.Token)));

            var mailbox = (Mailbox<TRequest>) mailboxLazy.Value;

            return mailbox.Post(request);
        }

        public Task Complete(params Type[] order)
        {
            var mailboxes = order.Length == 0 ? Mailboxes.Keys : order;

            foreach (var mbKey in mailboxes)
            {
                if (Mailboxes.TryGetValue(mbKey, out var mailbox))
                {
                    mailbox.Value.Complete();
                }
            }

            return Task.WhenAll(Mailboxes.Values.Select(s => s.Value.Completion).ToList());
        }

        public void Terminate()
        {
            cts.Cancel();
        }
    }
}