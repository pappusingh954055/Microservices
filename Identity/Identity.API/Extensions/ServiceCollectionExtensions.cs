using FluentValidation;
using System.Reflection;

namespace Identity.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                Assembly.Load("Identity.Application")));

        services.AddValidatorsFromAssembly(
            Assembly.Load("Identity.Application"));

        return services;
    }
}
