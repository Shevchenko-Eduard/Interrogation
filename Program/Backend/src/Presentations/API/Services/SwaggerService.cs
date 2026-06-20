using API.Schema;
using Microsoft.OpenApi;

namespace API.Services;

public static class SwaggerService
{
    public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type =>
                type.FullName?.Replace("+", ".") ?? type.Name);

            const string referenceId = "Keycloak";

            KeycloakSchema keycloakSchema = new(configuration);

            var authUrl = keycloakSchema.AuthorizationEndpoint().GetAwaiter().GetResult();
            var tokenUrl = keycloakSchema.TokenEndpoint().GetAwaiter().GetResult();

            OpenApiSecurityScheme openApiSecurityScheme = new()
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(authUrl),
                        TokenUrl = new Uri(tokenUrl),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID" },
                            { "profile", "Profile" },
                            { "roles", "Roles" }
                        }
                    }
                }
            };

            options.AddSecurityDefinition(referenceId, openApiSecurityScheme);

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(referenceId, document)] = ["openid", "profile", "roles"]
            });
        });

        return services;
    }

    public static WebApplication UseSwaggerMap(this WebApplication app, IConfiguration configuration)
    {
        KeycloakSchema schema = new(configuration);

        app.UseSwagger(c =>
        {
            c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
            {
                var scheme = httpReq.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? httpReq.Scheme;
                var host = httpReq.Headers["X-Forwarded-Host"].FirstOrDefault() ?? httpReq.Host.Host;
                var pathBase = httpReq.Headers["X-Forwarded-Prefix"].FirstOrDefault() ?? "";

                swaggerDoc.Servers =
                [
                    new OpenApiServer { Url = $"{scheme}://{host}{pathBase}" }
                ];
            });
        });
        app.UseSwaggerUI(options =>
        {
            options.OAuthClientId(schema.Resource);
            options.OAuthClientSecret(schema.Secret);
            options.OAuthRealm(schema.Realm);
            options.OAuthScopes("openid", "profile", "roles");
            // options.OAuthUsePkce();

            var externalUrl = string.Empty;
            var httpContext = app.Services.GetRequiredService<IHttpContextAccessor>().HttpContext;
            if (httpContext?.Request != null)
            {
                var scheme = httpContext.Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? "https";
                var host = httpContext.Request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? httpContext.Request.Host.Host;
                var pathBase = httpContext.Request.Headers["X-Forwarded-Prefix"].FirstOrDefault() ?? "";
                externalUrl = $"{scheme}://{host}{pathBase}";
            }
            if (!string.IsNullOrEmpty(externalUrl))
            {
                var redirectUri = $"{externalUrl}/swagger/oauth2-redirect.html";
                options.OAuth2RedirectUrl(redirectUri);
            }
        });
        return app;
    }
}