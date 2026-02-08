using FluentValidation;
using Identity.Application.Interfaces;
using Identity.Application.Services;
using System.Reflection;

namespace Identity.API.Extensions;

public static class ServicesCollectionExtensions
{
    public static IServiceCollection AddIdentityApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("Identity.Application")));
        services.AddValidatorsFromAssembly(Assembly.Load("Identity.Application"));

        services.AddScoped<IMenuService, MenuService>();

        return services;
    }
}
