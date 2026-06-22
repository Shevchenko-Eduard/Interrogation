using API.Schema;
using Infrastructure.Database;
using Infrastructure.Database.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API.Services;

/// <summary>
/// Должны быть переменные окружения:
/// DB__Host,
/// DB__Port,
/// DB__Database,
/// DB__Username,
/// DB__Password,
/// </summary>
internal static class PostgresService
{
    public static IServiceCollection AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        PostgresSchema postgresSchema = new(configuration);
        services.AddPooledDbContextFactory<ProgramContext>(options => options.UseNpgsql(GetConnectionString(postgresSchema)));
        services.AddScoped<IDatabaseInitialization, DatabaseInitialization>();
        return services;
    }
    private static string GetConnectionString(PostgresSchema postgresSchema)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = postgresSchema.Host,
            Port = postgresSchema.Port,
            Database = postgresSchema.Database,
            Username = postgresSchema.Username,
            Password = postgresSchema.Password
        };

        return builder.ToString();
    }
}