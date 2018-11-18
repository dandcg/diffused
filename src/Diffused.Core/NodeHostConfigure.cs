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

            services.AddMediatrScoped();

            services.AddScoped<IRequestHandler<Test, Unit>, TestHandler>();
        }

        private static void AddMediatrScoped(this IServiceCollection services)
        {
            services.AddScoped<ServiceFactory>(p => p.GetService);
            services.Add(new ServiceDescriptor(typeof(IMediator), typeof(Mediator), ServiceLifetime.Scoped));
        }
    }
}