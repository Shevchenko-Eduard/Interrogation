using API.Schema;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

namespace API.Services;

/// <summary>
/// Должны быть переменные окружения:
/// Redis__Endpoint в формате host:port,
/// Redis__InstanceName (опционально),
/// Redis__Password (опционально)
/// </summary>
public static class RedisService
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        RedisSchema redisSchema = new(configuration);

        var redisConfigurationOptions = new ConfigurationOptions
        {
            EndPoints = { redisSchema.Endpoint },
            Password = redisSchema.Password
        };

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisSchema.Endpoint;
            options.InstanceName = redisSchema.InstanceName;
            if (!string.IsNullOrEmpty(redisSchema.Password))
            {
                options.ConfigurationOptions = redisConfigurationOptions;
            }
        });

        services.AddStackExchangeRedisOutputCache(options =>
        {
            options.Configuration = redisSchema.Endpoint;
            options.InstanceName = redisSchema.InstanceName;
            if (!string.IsNullOrEmpty(redisSchema.Password))
            {
                options.ConfigurationOptions = redisConfigurationOptions;
            }
        });

        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfigurationOptions);

        services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(connectionMultiplexer, redisSchema.Password)
            .UnprotectKeysWithAnyCertificate(); // Не шифровать ключи

        return services;
    }
}