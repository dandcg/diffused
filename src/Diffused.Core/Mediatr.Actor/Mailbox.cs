using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MediatR;

namespace Diffused.Core.Mediatr.Actor
{
    public class Mailbox<TRequest> : IMailbox
    {
        private readonly ActionBlock<TRequest> queue;

        public Mailbox(IMediator mediator)
        {
            queue = new ActionBlock<TRequest>(async request => await mediator.Send((IRequest<ActorResult>) request));
        }

        public Task<bool> Send(TRequest request)
        {
            var actorRequest = request as ActorRequest;
            actorRequest?.SetHandleNow();

            return queue.SendAsync(request);
        }

        public Task<bool> Post(TRequest request)
        {
            var actorRequest = request as ActorRequest;
            actorRequest?.SetHandleNow();

            return Task.FromResult(queue.Post(request));
        }
    }
}