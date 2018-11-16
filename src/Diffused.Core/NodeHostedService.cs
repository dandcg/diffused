using System;
using System.Collections.Generic;
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

        internal List<Node> HostedNodes = new List<Node>();

        public NodeHostedService(IServiceProvider services, ILogger<NodeHostedService> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Node Hosted Service is starting.");

            using (var scope = services.CreateScope())
            {
                var node = scope.ServiceProvider.GetRequiredService<Node>();

                await node.StartAsync(cancellationToken);

                HostedNodes.Add(node);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Node Hosted Service is stopping.");

            var tasks = new List<Task>();

            foreach (var node in HostedNodes)
            {
                tasks.Add(node.StopAsync(cancellationToken));
            }

            await Task.WhenAll(tasks);

            logger.LogInformation("Node Hosted Service is stopped.");
        }
    }
}