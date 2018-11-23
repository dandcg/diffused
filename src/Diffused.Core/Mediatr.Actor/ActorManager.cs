using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        internal ConcurrentDictionary<Type, Lazy<IActorController>> ActorControllers { get; } = new ConcurrentDictionary<Type, Lazy<IActorController>>();

        public Task<bool> Send<TRequest>(TRequest request)
        {
            var mailboxLazy = ActorControllers.GetOrAdd(typeof(TRequest), key => new Lazy<IActorController>(() => new ActorController<TRequest>(mediator, cts.Token)));

            var mailbox = (ActorController<TRequest>) mailboxLazy.Value;

            return mailbox.Send(request);
        }

        public Task<bool> Post<TRequest>(TRequest request)
        {
            var mailboxLazy = ActorControllers.GetOrAdd(typeof(TRequest), key => new Lazy<IActorController>(() => new ActorController<TRequest>(mediator, cts.Token)));

            var mailbox = (ActorController<TRequest>) mailboxLazy.Value;

            return mailbox.Post(request);
        }

        public async Task Finished()
        {
            IEnumerable<IActorController> actors;
            do
            {
                actors = ActorControllers.Values.Select(s => s.Value).ToList();
                await Task.Delay(100);
            } while (actors.Sum(s => s.InputCount) > 0);

            foreach (var actor in actors)
            {
                actor.Complete();
            }

            await Task.WhenAll(actors.Select(s => s.Completion).ToList());
        }

        public void Terminate(params Type[] keys)
        {
            foreach (var key in keys)
            {
                if (ActorControllers.TryGetValue(key, out var mailbox))
                {
                    mailbox.Value.Cancel();
                }
            }
        }

        public void TerminateAll()
        {
            cts.Cancel();
        }
    }
}