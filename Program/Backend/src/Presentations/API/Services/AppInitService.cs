using API.DependencyInjection;
using LibWeb.Services;

namespace API.Services;

internal static class AppInitService
{
    public static WebApplicationBuilder AddAppInit(this WebApplicationBuilder builder)
    {
        builder.Logging.AddConsole();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddPostgres(builder.Configuration);
        builder.Services.AddMinioClient(builder.Configuration);
        builder.Services.AddApplicationServices();

        // builder.Services.AddRedis(builder.Configuration);
        builder.Services.AddKeycloak(builder.Configuration);
        builder.Services.AddRepositories();

        builder.Services.AddHostedService<DbInitService>();

        return builder;
    }
}