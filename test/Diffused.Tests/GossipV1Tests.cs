using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Diffused.Core;
using Diffused.Core.Implementations.GossipV1;
using Diffused.Core.Implementations.Test;
using Diffused.Core.Infrastructure;
using Diffused.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Diffused.Tests
{
    public class GossipV1Tests:IDisposable
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
            var node1 = nodeFactory.CreateGossipV1(new GossipV1NodeConfig());

            await node1.StartAsync();

            await node1.StopAsync();

        }

        public void Dispose()
        {
            serviceProvider?.Dispose();
            nodeFactory?.Dispose();
        }
    }
}