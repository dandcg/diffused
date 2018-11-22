using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Diffused.Core.Handlers;
using Diffused.Core.Mediatr.Actor;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Diffused.Core.NodeImpl
{
    public class Node
    {
        private readonly ILogger<NodeHostedService> logger;
        private readonly IMediator mediator;
        private readonly ActorManager actorManager;

        internal Guid NodeId = Guid.NewGuid();




        private Task _executingTask;
        private CancellationTokenSource _cts;


        public Node(ILogger<NodeHostedService> logger, IMediator mediator, ActorManager actorManager)
        {
            this.logger = logger;
            this.mediator = mediator;
            this.actorManager = actorManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Node {NodeId} is starting.", NodeId);

            await mediator.Send(new Test(NodeId));
            await mediator.Send(new Test(NodeId));
            await mediator.Send(new Test(NodeId));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Node {NodeId} is stopping.", NodeId);

            await actorManager.Complete();
            
            logger.LogInformation("Node {NodeId} is stopped.", NodeId);
        }
    }
}