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
        private CancellationTokenSource cts;

        public NodeHostedService(IServiceProvider services, ILogger<NodeHostedService> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            scope = services.CreateScope();

            node = scope.ServiceProvider.GetRequiredService<Node>();

            executingTask = node.RunAsync();

            return executingTask.IsCompleted ? executingTask : Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (executingTask == null)
            {
                return;
            }

            await Task.WhenAny(node.StopAsync(), Task.Delay(-1, cancellationToken));

            cancellationToken.ThrowIfCancellationRequested();

            scope.Dispose();
        }
    }
}