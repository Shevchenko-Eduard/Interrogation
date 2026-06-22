using API.Schema;
using Minio;

namespace API.Services;

/// <summary>
/// Должны быть переменные окружения:
/// Minio__Endpoint в формате host:port,
/// Minio__Username,
/// Minio__Password, 
/// Minio__HTTPS(не обязательно) использовать ли https, по умолчанию false.
/// </summary>
internal static class MinioService
{
    public static IServiceCollection AddMinioClient(this IServiceCollection services, IConfiguration configuration)
    {
        MinIoSchema minIoSchema = new(configuration);

        services.AddMinio(configureClient => configureClient
            .WithEndpoint(minIoSchema.Endpoint)
            .WithCredentials(minIoSchema.Username, minIoSchema.Password)
            .WithSSL(minIoSchema.Https));

        return services;
    }
}