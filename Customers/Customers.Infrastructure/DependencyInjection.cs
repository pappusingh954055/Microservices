using Customers.Application.Common.Interfaces;
using Customers.Infrastructure.Persistence;
using Customers.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Customers.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<CustomerDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CustomersDb")));

            services.AddScoped<ICustomerRepository, CustomerRepository>();


            //services.AddScoped<CustomerDbContext>(
            //provider => provider.GetRequiredService<CustomerDbContext>());

            

            return services;
        }
    }
}
