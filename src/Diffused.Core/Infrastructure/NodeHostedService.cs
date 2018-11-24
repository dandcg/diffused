using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Diffused.Core.Infrastructure
{
    public class NodeHostedService : IHostedService, IDisposable
    {
        private readonly ILogger logger;
        private readonly NodeFactory nodeFactory;
        private INode node;
        private CancellationTokenSource cts;

        public NodeHostedService(ILogger<NodeHostedService> logger, NodeFactory nodeFactory)
        {
            this.logger = logger;
            this.nodeFactory = nodeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            node = nodeFactory.CreateTestNode();
            return node.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAny(node.StopAsync(), Task.Delay(-1, cancellationToken));
            cancellationToken.ThrowIfCancellationRequested();
        }

        public void Dispose()
        {
            nodeFactory?.Dispose();
        }
    }
}