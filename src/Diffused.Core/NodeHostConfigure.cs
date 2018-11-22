using Diffused.Core.Handlers;
using Diffused.Core.Mediatr.Actor;
using Diffused.Core.NodeImpl;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Diffused.Core
{
    public static class NodeHostConfigure
    {
        public static void AddNodeSerivces(this IServiceCollection services)

        {
            services.AddHostedService<NodeHostedService>();

            services.AddScoped<Node>();
            services.AddScoped<ActorManager>();

            services.AddMediatR(typeof(TestHandler));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ActorBehaviour<,>));
        }
    }
}