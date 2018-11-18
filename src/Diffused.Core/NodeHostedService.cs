using System;
using System.Threading;
using System.Threading.Tasks;
using Diffused.Core.NodeImpl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Diffused.Core
{
    public class NodeHostedService : IHostedService
    {
        private readonly ILogger logger;

        private readonly IServiceProvider services;

        internal Node Node;

        public NodeHostedService(IServiceProvider services, ILogger<NodeHostedService> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = services.CreateScope())
            {
                Node = scope.ServiceProvider.GetRequiredService<Node>();

                await Node.StartAsync(cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Node.StopAsync(cancellationToken);
        }
    }
}