using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MediatR;

namespace Diffused.Core.Mediatr.Actor
{
    public class ActorManager
    {
        private readonly IMediator mediator;

        public ActorManager(IMediator mediator)
        {
            this.mediator = mediator;
        }

        internal ConcurrentDictionary<Type, Lazy<IMailbox>> Mailboxes { get; } = new ConcurrentDictionary<Type, Lazy<IMailbox>>();

        public Task<bool> Send<TRequest>(TRequest request)
        {
            var mailboxLazy = Mailboxes.GetOrAdd(typeof(TRequest), key => new Lazy<IMailbox>(() => new Mailbox<TRequest>(mediator)));

            var mailbox = (Mailbox<TRequest>) mailboxLazy.Value;

            return mailbox.Send(request);
        }

        public Task<bool> Post<TRequest>(TRequest request)
        {
            var mailboxLazy = Mailboxes.GetOrAdd(typeof(TRequest), key => new Lazy<IMailbox>(() => new Mailbox<TRequest>(mediator)));

            var mailbox = (Mailbox<TRequest>) mailboxLazy.Value;

            return mailbox.Post(request);
        }
    }
}