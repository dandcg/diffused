using System.Threading;
using System.Threading.Tasks;
using Diffused.Core;
using Diffused.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Diffused.Tests
{
    public class InfrastructureTests
    {
        
        private readonly ServiceCollection services;

        public InfrastructureTests(ITestOutputHelper output)
        {
            // services

            services = new ServiceCollection();

            // logging
            
            var logger = output.SetupLogging().ForContext("SourceContext", "InfrastructureTests");
            
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger, true));
            services.AddNodeSerivces();

        }

        [Fact]
        public async Task Test1()
        {
            var serviceProvider = services.BuildServiceProvider();

            var service = serviceProvider.GetService<IHostedService>() as NodeHostedService;

            await service.StartAsync(CancellationToken.None);

            await service.StopAsync(CancellationToken.None);
        }
    }
}