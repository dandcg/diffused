using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MediatR;

namespace Diffused.Core.Mediatr.Actor
{
    public class ActorController<TRequest> : IActorController
    {
        private readonly ActionBlock<TRequest> queue;
        private readonly CancellationTokenSource cts;
        private volatile bool cancelled;

        public ActorController(IMediator mediator, CancellationToken cancellationToken)
        {
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            queue = new ActionBlock<TRequest>(async request =>
            {
                if (!cancelled)
                {
                    await mediator.Send((IRequest<ActorResult>) request, cts.Token);
                }
            });
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

        public int InputCount => queue.InputCount;

        public void Complete()
        {
            queue.Complete();
        }

        public Task Completion => queue.Completion;

        public void Cancel()
        {
            cancelled = true;

            cts.Cancel();
        }
    }
}