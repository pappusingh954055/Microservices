using Company.Application.Common.Interfaces;
using Company.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<CompanyDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CompanyDb")));

            services.AddScoped<ICompanyRepository, CompanyRepository>();

            return services;
        }
    }
}