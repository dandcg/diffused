using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Diffused.Core.ActorImpl
{
    public class TestHandler:IRequestHandler<Test>
    {
        private readonly ILogger<NodeHostedService> logger;
        private readonly IMediator mediator;
        internal Guid HandlerId = Guid.NewGuid();

        public TestHandler(ILogger<NodeHostedService> logger, IMediator mediator)
        {
            this.logger = logger;
            this.mediator = mediator;
        }

        public Task<Unit> Handle(Test request, CancellationToken cancellationToken)
        {
           logger.LogInformation("Test Request {NodeId} with {HandlerId}",request.NodeId,HandlerId);
            return null;
        }
    }
}