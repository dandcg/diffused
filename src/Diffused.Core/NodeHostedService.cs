using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Diffused.Core
{
    public class NodeHostedService : IHostedService
    {
        private readonly ILogger logger;
        private Node scopedProcessingService;

        public NodeHostedService(IServiceProvider services, ILogger<NodeHostedService> logger)
        {
            Services = services;
            this.logger = logger;
        }

        public IServiceProvider Services { get; }

        public  async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Node Hosted Service is starting.");

            using (var scope = Services.CreateScope())
            {
                scopedProcessingService = scope.ServiceProvider.GetRequiredService<Node>();
                
                await scopedProcessingService.BootstrapBlock.SendAsync(new Bootstrap(), cancellationToken);
                scopedProcessingService.BootstrapBlock.Complete();
            }

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Node Hosted Service is stopping.");

            await scopedProcessingService.BootstrapBlock.Completion;


        }


     


    }
}
