using Common.ServiceAttribute;
using Microsoft.Extensions.DependencyInjection;

namespace TestApiNet8.Application
{
    public static class DependencyInjection
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);
            services.AddCustomServices();
        }
    }
}
