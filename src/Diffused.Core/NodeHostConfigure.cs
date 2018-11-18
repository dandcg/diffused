using System;
using System.Collections.Generic;
using System.Linq;
using Diffused.Core.ActorImpl;
using Diffused.Core.NodeImpl;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Diffused.Core
{
    public static class NodeHostConfigure
    {
        public static void AddNodeSerivces(this IServiceCollection services)

        {
            services.AddHostedService<NodeHostedService>();

            services.AddScoped<Node>();

            services.AddScoped<IRequestHandler<Test, Unit>, TestHandler>();

            services.AddMediatrScoped();
        }

        private static void AddMediatrScoped(this IServiceCollection services)
        {
            services.AddScoped<ServiceFactory>(p => type =>
            {
                try
                {
                    return p.GetService(type);
                }
                catch (ArgumentException)
                {
                    // Let's assume it's a constrained generic type
                    if (type.IsConstructedGenericType &&
                        type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        var serviceType = type.GenericTypeArguments.Single();
                        var serviceTypes = new List<Type>();
                        foreach (var service in services.ToList())
                        {
                            if (serviceType.IsConstructedGenericType &&
                                serviceType.GetGenericTypeDefinition() == service.ServiceType)
                            {
                                try
                                {
                                    var closedImplType = service.ImplementationType.MakeGenericType(serviceType.GenericTypeArguments);
                                    serviceTypes.Add(closedImplType);
                                }
                                catch (ArgumentException)
                                {
                                }
                            }
                        }

                        services.Replace(new ServiceDescriptor(type, sp => serviceTypes.Select(sp.GetService).ToArray(), ServiceLifetime.Transient));

                        var resolved = Array.CreateInstance(serviceType, serviceTypes.Count);

                        Array.Copy(serviceTypes.Select(p.GetService).ToArray(), resolved, serviceTypes.Count);

                        return resolved;
                    }

                    throw;
                }
            });
            services.Add(new ServiceDescriptor(typeof(IMediator), typeof(Mediator), ServiceLifetime.Scoped));
        }
    }
}