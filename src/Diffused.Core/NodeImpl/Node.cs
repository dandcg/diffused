using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Diffused.Core.NodeImpl
{
    public class Node
    {

        private readonly ILogger<NodeHostedService> logger;

        internal Guid NodeId = Guid.NewGuid();
        
        internal ActionBlock<Bootstrap> BootstrapBlock;

        public Node(ILogger<NodeHostedService> logger, IMediator mediator)
        {
            this.logger = logger;
            BootstrapBlock = new ActionBlock<Bootstrap>(
                b =>
                {
                    

                    
                    
                    
                    logger.LogInformation("Bootstrap");
                });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Node {NodeId} is starting.",NodeId);

           
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Node {NodeId} is stopping.",NodeId);
            BootstrapBlock.Complete();
            await BootstrapBlock.Completion;
            logger.LogInformation("Node {NodeId} is stopped.",NodeId);
        }
    }
}