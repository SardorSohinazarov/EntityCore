using Microsoft.Extensions.DependencyInjection;
using TestApiWithNet8;

namespace TestApiNet8.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>();

            return services;
        }
    }
}
