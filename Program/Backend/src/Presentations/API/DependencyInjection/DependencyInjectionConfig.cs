using Application.Interfaces;
using Domain.Interfaces;
using Infrastructure;
using LibWeb;

namespace API.DependencyInjection;

public static partial class DependencyInjectionConfig
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IClock, Clock>();

        services.AddScoped<IDocumentKeyManager, DocumentKeyManager>();

        services.AddScoped<ISecretManager, SecretManager>();

        services.AddScoped<Infrastructure.Interfaces.IHostEnvironment, HostEnvironment>();

        services.AddScoped<ICurrentUser, CurrentUserService>();

        return services;
    }
}