using Microsoft.Extensions.DependencyInjection;
using Services.Students;

namespace TestApiNet8.Application
{
    public static class DependencyInjection
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);
            services.AddScoped<IStudentsService, StudentsService>();
        }
    }
}
