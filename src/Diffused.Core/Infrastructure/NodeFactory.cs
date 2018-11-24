using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Diffused.Core.Implementations.GossipV1;
using Diffused.Core.Implementations.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Diffused.Core.Infrastructure
{
    public class NodeFactory:IDisposable
    {
        private readonly IServiceProvider services;
        private readonly ILogger<NodeFactory> logger;
        private readonly List<IServiceScope> scopes = new List<IServiceScope>();
        public NodeFactory(IServiceProvider services, ILogger<NodeFactory> logger)
        {
            this.services = services;
            this.logger = logger;
        }
        public INode CreateTestNode()
        {
            var scope = services.CreateScope();
            scopes.Add(scope);
            return scope.ServiceProvider.GetRequiredService<TestNode>();
        }

        public INode CreateGossipV1(GossipV1NodeConfig config)
        {
            var scope = services.CreateScope();
            scopes.Add(scope);
            var node = scope.ServiceProvider.GetRequiredService<GossipV1Node>();
            node.Configure(config);
            return node;
        }

        public void Dispose()
        {
            foreach (var scope in scopes)
            {
            scope?.Dispose();
            }
        }
    }
}
