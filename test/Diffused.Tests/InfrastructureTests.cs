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
        private ServiceProvider serviceProvider;

        public InfrastructureTests(ITestOutputHelper output)
        {
            // services

            services = new ServiceCollection();

            // logging

            var logger = output.SetupLogging().ForContext("SourceContext", "InfrastructureTests");

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger, true));
            services.AddNodeSerivces();
        }

        public T GetService<T>()
        {
            if (serviceProvider == null)
            {
                serviceProvider = services.BuildServiceProvider();
            }

            return serviceProvider.GetService<T>();
        }

        [Fact]
        public async Task Test1()
        {
            var service = GetService<IHostedService>();

            await service.StartAsync(CancellationToken.None);

            await service.StopAsync(CancellationToken.None);
        }
    }
}