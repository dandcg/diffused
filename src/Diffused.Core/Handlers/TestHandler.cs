using System;
using System.Threading;
using System.Threading.Tasks;
using Diffused.Core.Mediatr.Actor;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Diffused.Core.Handlers
{
    public class TestHandler : IRequestHandler<Test, ActorResult>
    {
        private readonly ILogger<NodeHostedService> logger;
        private readonly IMediator mediator;
        internal Guid HandlerId = Guid.NewGuid();

        public TestHandler(ILogger<NodeHostedService> logger, IMediator mediator)
        {
            this.logger = logger;
            this.mediator = mediator;
        }

        public Task<ActorResult> Handle(Test request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Test Request {NodeId} with {HandlerId}", request.NodeId, HandlerId);
            return Task.FromResult(new ActorResult());
        }
    }
}