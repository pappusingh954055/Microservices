using Inventory.Application.Common.Interfaces;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            services.AddScoped<IGRNRepository, GRNRepository>();
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ISaleOrderRepository, SaleOrderRepository>();

            // Program.cs ke andar builder.Services mein add karein
            services.AddScoped<IPurchaseReturnRepository, PurchaseReturnRepository>();

            services.AddScoped<IInventoryDbContext>(
            provider => provider.GetRequiredService<InventoryDbContext>());

            return services;
        }
    }
}
