
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Common.ServiceAttribute
{
    public static class ServiceAttributeCollectionExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            var types = Assembly.GetExecutingAssembly()
                .GetTypes().Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                var serviceAttribute = type.GetCustomAttribute<ServiceAttribute>();
                if (serviceAttribute == null) continue;

                var interfaceType = type.GetInterfaces().FirstOrDefault();
                if (interfaceType is null)
                {
                    switch (serviceAttribute.Lifetime)
                    {
                        case ServiceLifetime.Scoped:
                            services.AddScoped(type);
                            break;
                        case ServiceLifetime.Transient:
                            services.AddTransient(type);
                            break;
                        case ServiceLifetime.Singleton:
                            services.AddSingleton(type);
                            break;
                    }
                }
                else
                {
                    switch (serviceAttribute.Lifetime)
                    {
                        case ServiceLifetime.Scoped:
                            services.AddScoped(interfaceType, type);
                            break;
                        case ServiceLifetime.Transient:
                            services.AddTransient(interfaceType, type);
                            break;
                        case ServiceLifetime.Singleton:
                            services.AddSingleton(interfaceType, type);
                            break;
                    }
                }
            }

            return services;
        }
    }
}
