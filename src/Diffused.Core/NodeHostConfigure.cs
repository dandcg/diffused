using Microsoft.Extensions.DependencyInjection;

namespace Diffused.Core
{
    public static class NodeHostConfigure
    {
        public static void AddNodeSerivces(this IServiceCollection services)

        {
            services.AddHostedService<NodeHostedService>();
            services.AddSingleton<Node>();
        }
    }
}