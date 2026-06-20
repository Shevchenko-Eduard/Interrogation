using API.Schema;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;

namespace LibWeb.Services;

/// <summary>
/// Должны быть переменные окружения:
/// Keycloak__AuthServerUrl,
/// Keycloak__Realm,
/// Keycloak__Resource,
/// Keycloak__Secret,
/// Keycloak__SslRequired(не обязательно),
/// Keycloak__VerifyTokenAudience(не обязательно).
/// </summary>
public static class KeycloakService
{
    public static IServiceCollection AddKeycloak(this IServiceCollection services, IConfiguration configuration)
    {
        KeycloakSchema schema = new(configuration);
        services.AddKeycloakAuthentication(schema);
        services.AddKeycloakAuthorization(schema);
        return services;
    }
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services, KeycloakSchema schema)
    {
        services
            .AddAuthentication()
            .AddKeycloakWebApi(configureKeycloakOptions: options =>
        {
            options.AuthServerUrl = schema.AuthServerUrl;
            options.Realm = schema.Realm;
            options.Resource = schema.Resource;
            options.Credentials = new() { Secret = schema.Secret };
            options.SslRequired = schema.SslRequired;
            options.VerifyTokenAudience = schema.VerifyTokenAudience;
            options.RoleClaimType = KeycloakSchema.RoleClaimType;
        },
        configureJwtBearerOptions: jwtOptions =>
        {
            jwtOptions.MetadataAddress = schema.MetadataAddress;
            jwtOptions.BackchannelHttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            jwtOptions.RequireHttpsMetadata = false;

            jwtOptions.TokenValidationParameters = new()
            {
                ValidateAudience = true,
                ValidAudience = "account",

                RoleClaimType = KeycloakSchema.RoleClaimType,

                ValidateIssuer = true,
                ValidIssuer = schema.IssuerEndpoint().GetAwaiter().GetResult(),

                ValidateLifetime = true
            };
        });

        return services;
    }
    public static IServiceCollection AddKeycloakAuthorization(
        this IServiceCollection services, KeycloakSchema schema)
    {
        services
            .AddAuthorization()
            .AddKeycloakAuthorization(configureKeycloakAuthorizationOptions: options =>
        {
            options.AuthServerUrl = schema.AuthServerUrl;
            options.Realm = schema.Realm;
            options.Resource = schema.Resource;
            options.Credentials = new() { Secret = schema.Secret };
            options.SslRequired = schema.SslRequired;
            options.VerifyTokenAudience = schema.VerifyTokenAudience;
            options.EnableRolesMapping = RolesClaimTransformationSource.ResourceAccess;
            options.RoleClaimType = KeycloakSchema.RoleClaimType;
        })
            .AddAuthorizationBuilder();

        return services;
    }
}