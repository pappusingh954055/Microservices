using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Repositories;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<InventoryDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("InventoryDb")));

            services.AddScoped<ICategoryRepository, CategoryRepository>();

            services.AddScoped<ISubcategoryRepository, SubcategoryRepository>();

            services.AddScoped<IProductRepository, ProductRepository>();

            services.AddScoped<IPriceListRepository, PriceListRepository>();

            return services;
        }
    }
}
