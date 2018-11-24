using System;
using System.Linq;
using System.Threading.Tasks;
using Diffused.Core.Implementations.Swim.Model;
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
        public async Task SwimNodeTest1()
        {

            var n =10;

            var nodeCollection = NodeCollection.Create(nodeFactory, n);

            for (int j = 1; j < n; j++)
            {
                nodeCollection[j].SeedMembers = new[] {nodeCollection[j-1].Self.Address};
            }

            await nodeCollection.StartAllAsync();

            int i = 0;
            do
            {
                i = (int) nodeCollection.Average(s => s.Members.Count);
                await Task.Delay(100);
            } while (i < n-1);

            

            await nodeCollection.StopAllAsync();
        }

        
        [Fact]
        public async Task SwimNodeTest2()
        {

            var n= 5;

            var nodeCollection = NodeCollection.Create(nodeFactory, n);

            for (int j = 1; j < n; j++)
            {
                nodeCollection[j].SeedMembers = new[] {nodeCollection[j-1].Self.Address};
            }

            await nodeCollection.StartAllAsync();

           
            int i = 0;
            do
            {
                i = (int) nodeCollection.Average(s => s.Members.Count);
                await Task.Delay(100);
            } while (i < n-1);

            await nodeCollection[0].StopAsync();
            i = 0;
            do
            {
                i = (int) nodeCollection.SelectMany(s => s.Members).Count(w => w.State == MemberState.Dead);
            } while (i !=4);

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