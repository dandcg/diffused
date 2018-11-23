using Diffused.Core.Actors.Test;
using Diffused.Core.Implementations.GossipV1;
using Diffused.Core.Implementations.Test;
using Diffused.Core.Mediatr.Actor;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Diffused.Core.Infrastructure
{
    public static class NodeHostConfigure
    {
        public static void AddNodeServices(this IServiceCollection services)

        {
            services.AddHostedService<NodeHostedService>();

            services.AddScoped<ActorManager>();

            services.AddMediatR(typeof(TestHandler));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ActorBehaviour<,>));

            services.AddScoped<TestNode>();
            services.AddScoped<GossipV1Node>();
            services.AddScoped<GossipV1NodeConfig>();

            services.AddScoped<ITransportFactory, TransportFactory>();
            services.AddSingleton<InMemRouter>();
        }
    }
}