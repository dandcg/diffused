using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Diffused.Core.Infrastructure
{
    public class NodeHostedService : IHostedService
    {
        private readonly ILogger logger;
        private readonly IServiceProvider services;
        private INode node;
        private IServiceScope scope;
        private Task executingTask;
        private CancellationTokenSource cts;

        public NodeHostedService(IServiceProvider services, ILogger<NodeHostedService> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        public Type NodeType { get; set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            scope = services.CreateScope();

            node = scope.ServiceProvider.GetRequiredService(NodeType) as INode;

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