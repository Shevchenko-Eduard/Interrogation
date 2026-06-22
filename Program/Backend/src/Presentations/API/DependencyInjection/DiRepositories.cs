using Application.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Repositories.BaseRepository;
using Infrastructure.EfRepository;
using Infrastructure.MinioRepository;

namespace API.DependencyInjection;

internal static partial class DependencyInjectionConfig
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<IS3Repository, MinioRepository>();
        services.AddTransient<IS3DocumentRepository, MinioDocumentRepository>();

        services.AddTransient<IDocumentRepository, EfDocumentRepository>();
        services.AddTransient<IEncryptionTypeRepository, EfEncryptionTypeRepository>();
        services.AddTransient<IFragmentRepository, EfFragmentRepository>();
        services.AddTransient<ISecretRepository, EfSecretRepository>();

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}