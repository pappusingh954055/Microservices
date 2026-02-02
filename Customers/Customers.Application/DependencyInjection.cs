using Microsoft.Extensions.DependencyInjection;

namespace Customers.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssemblyContaining<AssemblyReference>());

            return services;
        }
    }
}
