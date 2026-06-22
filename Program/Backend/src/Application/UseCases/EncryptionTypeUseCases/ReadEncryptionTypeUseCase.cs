using Application.DTOs;
using Application.Interfaces;
using Domain.Entity;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.EncryptionTypeUseCases;

public class ReadEncryptionTypeUseCase(IEncryptionTypeRepository encryptionTypeRepository) : IQuestion<List<EncryptionTypeDTOs.Response.Read>, EncryptionTypeDTOs.Inner.ReadAll>
{
    private readonly IEncryptionTypeRepository _encryptionTypeRepository = encryptionTypeRepository;
    public async Task<List<EncryptionTypeDTOs.Response.Read>> Ask(EncryptionTypeDTOs.Inner.ReadAll input)
    {
        List<EncryptionType> encryptionTypes = await _encryptionTypeRepository.GetAllAsync().ConfigureAwait(false);
        return [.. encryptionTypes.Select(EncryptionTypeDTOs.Response.Read.FromEncryptionType)];
    }
}