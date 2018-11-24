using System;
using System.Threading.Tasks;
using Diffused.Core.Infrastructure;
using Diffused.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Diffused.Tests.Swim
{
    public class SwimTests : IDisposable
    {
        private readonly ServiceCollection services;
        private readonly ServiceProvider serviceProvider;
        private readonly NodeFactory nodeFactory;

        public SwimTests(ITestOutputHelper output)
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

            nodeCollection[1].SeedMembers = new[] {nodeCollection[0].Self.Address};
            //nodeCollection[2].SeedMembers = new[] {nodeCollection[1].Self.Address};

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