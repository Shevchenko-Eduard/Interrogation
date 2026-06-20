using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces.Repositories;

namespace Application.UseCases.EncryptionTypeUseCases;

public class ReadByIdEncryptionTypeUseCase(IEncryptionTypeRepository encryptionTypeRepository) : IQuestion<EncryptionTypeDTOs.Response.Read, EncryptionTypeDTOs.Inner.ReadById>
{
    private readonly IEncryptionTypeRepository _encryptionTypeRepository = encryptionTypeRepository;

    public async Task<EncryptionTypeDTOs.Response.Read> Ask(EncryptionTypeDTOs.Inner.ReadById input)
    {
        var encryptionType = await _encryptionTypeRepository.GetByIdAsync(input.Id)
            ?? throw new Exception($"Типа шифрования с таким Id:{input.Id} не существует.");
        return EncryptionTypeDTOs.Response.Read.FromEncryptionType(encryptionType);
    }
}