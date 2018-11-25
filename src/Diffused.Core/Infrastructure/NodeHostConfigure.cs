using Diffused.Core.Actors.Test;
using Diffused.Core.Implementations.Gossip.Swim;
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
            services.AddScoped<ActorManager>();
            services.AddMediatR(typeof(TestHandler));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ActorBehaviour<,>));

            services.AddHostedService<NodeHostedService>();
            services.AddTransient<NodeFactory>();

            services.AddScoped<TestNode>();
            services.AddScoped<SwimNode>();

            services.AddScoped<ITransportFactory, TransportFactory>();

            services.AddSingleton<InMemRouter>();
        }
    }
}