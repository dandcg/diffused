using Diffused.Core.ActorImpl;
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

            //services.AddScoped<INotificationHandler<Test>,TestHandler>();

            services.AddMediatR(typeof(TestHandler) );
        }
    }
}