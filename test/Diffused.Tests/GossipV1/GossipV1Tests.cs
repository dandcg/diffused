using System;
using System.Threading.Tasks;
using Diffused.Core.Infrastructure;
using Diffused.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Diffused.Tests.GossipV1
{
    public class GossipV1Tests : IDisposable
    {
        private readonly ServiceCollection services;
        private readonly ServiceProvider serviceProvider;
        private readonly NodeFactory nodeFactory;

        public GossipV1Tests(ITestOutputHelper output)
        {
            services = new ServiceCollection();

            // logging

            var logger = output.SetupLogging().ForContext("SourceContext", "InfrastructureTests");
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger, true));

            // services

            services.AddNodeServices();
            serviceProvider = services.BuildServiceProvider();
            nodeFactory = serviceProvider.GetService<NodeFactory>();
        }

        public T GetService<T>()
        {
            return serviceProvider.GetService<T>();
        }

        [Fact]
        public async Task GossipV1Node()
        {
            var nodeCollection = NodeCollection.Create(nodeFactory, 2);

            await nodeCollection.StartAllAsync();
            await Task.Delay(1000);
            await nodeCollection.StopAllAsync();
        }

        public void Dispose()
        {
            serviceProvider?.Dispose();
            nodeFactory?.Dispose();
        }
    }
}