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

        private Node node;
        private IServiceScope scope;
        private Task executingTask;

        public NodeHostedService(IServiceProvider services, ILogger<NodeHostedService> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            scope = services.CreateScope();

            node = scope.ServiceProvider.GetRequiredService<Node>();

            return node.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await node.StopAsync(cancellationToken);

            scope.Dispose();
        }
    }
}