using System;
using System.Threading.Tasks;
using Diffused.Core.Infrastructure;
using Diffused.Core.Mediatr.Actor;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Diffused.Core.Implementations.Test
{
    public class TestNode : INode
    {
        private readonly ILogger<NodeHostedService> logger;
        private readonly IMediator mediator;
        private readonly ActorManager actorManager;

        internal Guid NodeId = Guid.NewGuid();

        public TestNode(ILogger<NodeHostedService> logger, IMediator mediator, ActorManager actorManager)
        {
            this.logger = logger;
            this.mediator = mediator;
            this.actorManager = actorManager;
        }

        public async Task RunAsync()
        {
            logger.LogInformation("Node {NodeId} is starting.", NodeId);

            await mediator.Send(new Actors.Test.Test(NodeId));
            await mediator.Send(new Actors.Test.Test(NodeId));
            await mediator.Send(new Actors.Test.Test(NodeId));
        }

        public async Task StopAsync()
        {
            logger.LogInformation("Node {NodeId} is stopping.", NodeId);

            await actorManager.Finished();

            logger.LogInformation("Node {NodeId} is stopped.", NodeId);
        }
    }
}