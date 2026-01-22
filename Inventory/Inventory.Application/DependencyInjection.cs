using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Reflection.Metadata;

namespace Inventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<AssemblyReference>());

        return services;
    }
}
